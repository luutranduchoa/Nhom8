using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Medicine.Models;
using Medicine.DTOs;
using System.Diagnostics;

namespace Medicine.Controllers;

/// <summary>
/// Health check and diagnostics endpoint for monitoring API health
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(MedicalDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check - just returns OK if API is running
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Detailed health check including database connectivity
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        var startTime = Stopwatch.GetTimestamp();
        var checks = new Dictionary<string, object>();

        // API check
        checks["api"] = "ok";

        // Database check
        try
        {
            var dbStopwatch = Stopwatch.StartNew();
            await _context.Database.ExecuteSqlInterpolatedAsync($"SELECT 1");
            dbStopwatch.Stop();

            checks["database"] = new
            {
                status = "ok",
                responseTimeMs = dbStopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            checks["database"] = new
            {
                status = "error",
                message = ex.Message
            };
            return StatusCode(503, new
            {
                status = "unhealthy",
                checks = checks,
                timestamp = DateTime.UtcNow
            });
        }

        // Try to query some data
        try
        {
            var drugCount = await _context.Drugs.CountAsync();
            var diseaseCount = await _context.Diseases.CountAsync();

            checks["data"] = new
            {
                drugs = drugCount,
                diseases = diseaseCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve data counts");
            checks["data"] = new
            {
                status = "error",
                message = ex.Message
            };
        }

        var elapsed = Stopwatch.GetElapsedTime(startTime);

        return Ok(new
        {
            status = "healthy",
            checks = checks,
            responseTimeMs = elapsed.TotalMilliseconds,
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        });
    }

    /// <summary>
    /// Performance metrics and diagnostics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> Metrics()
    {
        try
        {
            var metrics = new
            {
                timestamp = DateTime.UtcNow,
                runtime = new
                {
                    uptime = GC.GetTotalMemory(false) / (1024 * 1024) + " MB",
                    processId = System.Diagnostics.Process.GetCurrentProcess().Id,
                    threadCount = System.Threading.ThreadPool.ThreadCount
                },
                database = new
                {
                    connectionPoolSize = _context.Database.GetConnectionString()?.Length ?? 0,
                    tables = new
                    {
                        drugs = _context.Drugs.Count(),
                        diseases = _context.Diseases.Count(),
                        indications = _context.Indications.Count(),
                        contraindications = _context.Contraindications.Count()
                    }
                }
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve metrics");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check if a specific service is available (e.g., for external dependencies)
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        try
        {
            // Check if database is ready
            await _context.Database.ExecuteSqlInterpolatedAsync($"SELECT 1");

            return Ok(new
            {
                ready = true,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service not ready");
            return StatusCode(503, new
            {
                ready = false,
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
