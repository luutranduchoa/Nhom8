import React, { useEffect, useState } from 'react';
import apiClient from './apiClient.js';
import { debugger as apiDebugger } from './debug.js';

const STORAGE_KEY = 'medicine_admin_diseases';
// API endpoint for diseases
const DISEASES_API = '/Disease';

const emptyDisease = {
  id: '',
  diseaseName: '',
  description: '',
  symptoms: '',
  recommendedMedications: '',
  treatmentGuidelines: '',
  prevention: '',
};

const parseList = (value) =>
  value
    .split('\n')
    .map((item) => item.trim())
    .filter(Boolean);

function DiseaseManagement() {
  const [diseases, setDiseases] = useState([]);
  const [form, setForm] = useState(emptyDisease);
  const [editingId, setEditingId] = useState(null);
  const [selectedId, setSelectedId] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  // Load diseases from localStorage on component mount
  useEffect(() => {
    try {
      const stored = window.localStorage.getItem(STORAGE_KEY);
      if (stored) {
        const parsed = JSON.parse(stored);
        if (Array.isArray(parsed)) {
          setDiseases(parsed);
          if (parsed.length > 0) {
            setSelectedId(parsed[0].id);
          }
        }
      }
    } catch {
      // ignore malformed storage
    }
  }, []);

  useEffect(() => {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(diseases));
  }, [diseases]);

  const selectedDisease = diseases.find((item) => item.id === selectedId) || null;

  const handleInputChange = (event) => {
    const { name, value } = event.target;
    setForm((current) => ({
      ...current,
      [name]: value,
    }));
  };

  const resetForm = () => {
    setForm(emptyDisease);
    setEditingId(null);
    setError(null);
  };

  const showError = (message) => {
    setError(message);
    setTimeout(() => setError(null), 5000);
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (!form.diseaseName.trim()) {
      window.alert('Tên bệnh là bắt buộc.');
      return;
    }

    const trimmedDisease = {
      ...form,
      diseaseName: form.diseaseName.trim(),
      description: form.description.trim(),
      symptoms: form.symptoms.trim(),
      recommendedMedications: form.recommendedMedications.trim(),
      treatmentGuidelines: form.treatmentGuidelines.trim(),
      prevention: form.prevention.trim(),
    };

    if (editingId) {
      // Update existing disease (localStorage + future API integration)
      setDiseases((current) =>
        current.map((item) => (item.id === editingId ? { ...item, ...trimmedDisease } : item)),
      );
      window.alert('Cập nhật bệnh thành công.');
    } else {
      // Add new disease (localStorage + future API integration)
      const newDisease = {
        ...trimmedDisease,
        id: `disease-${Date.now()}`,
      };
      setDiseases((current) => [newDisease, ...current]);
      setSelectedId(newDisease.id);
      window.alert('Thêm bệnh mới thành công.');
    }

    resetForm();
  };

  const handleEdit = (disease) => {
    setEditingId(disease.id);
    setForm({
      id: disease.id,
      diseaseName: disease.diseaseName,
      description: disease.description,
      symptoms: disease.symptoms,
      recommendedMedications: disease.recommendedMedications,
      treatmentGuidelines: disease.treatmentGuidelines,
      prevention: disease.prevention,
    });
    setSelectedId(disease.id);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleDelete = (id) => {
    const disease = diseases.find((item) => item.id === id);
    if (!disease) return;
    const confirmed = window.confirm(`Bạn có chắc chắn muốn xóa bệnh "${disease.diseaseName}"?`);
    if (!confirmed) return;

    setDiseases((current) => current.filter((item) => item.id !== id));
    if (selectedId === id) {
      setSelectedId(diseases.length > 1 ? diseases.filter((item) => item.id !== id)[0]?.id : null);
    }
  };

  const handleSelect = (id) => {
    setSelectedId(id);
  };

  return (
    <div className="min-h-screen bg-slate-50 p-6">
      <div className="max-w-7xl mx-auto space-y-6">
        {/* Error Toast */}
        {error && (
          <div className="fixed top-4 right-4 rounded-2xl bg-rose-50 border border-rose-200 px-4 py-3 text-sm text-rose-800 shadow-sm">
            {error}
          </div>
        )}

        {/* Loading Indicator */}
        {isLoading && (
          <div className="fixed top-4 left-4 rounded-2xl bg-sky-50 border border-sky-200 px-4 py-3 text-sm text-sky-800 shadow-sm">
            Đang tải...
          </div>
        )}

        <header className="rounded-3xl bg-white border border-slate-200 p-6 shadow-sm">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div>
              <p className="text-sm uppercase tracking-[0.2em] text-sky-600">Admin / Quản lý bệnh</p>
              <h1 className="mt-2 text-3xl font-semibold text-slate-900">Chi tiết bệnh</h1>
              <p className="mt-2 text-slate-600 max-w-2xl">Thêm, sửa, xóa bệnh và nội dung mô tả chi tiết gồm triệu chứng, thuốc khuyến nghị, hướng dẫn điều trị và cách phòng ngừa.</p>
            </div>
            <div className="flex flex-wrap gap-3">
              <button
                type="button"
                onClick={resetForm}
                className="rounded-2xl border border-slate-300 bg-white px-5 py-3 text-sm font-semibold text-slate-700 shadow-sm transition hover:border-slate-400 hover:bg-slate-50"
              >
                Khởi tạo form
              </button>
              <span className="inline-flex items-center rounded-2xl bg-slate-100 px-4 py-3 text-sm text-slate-600">
                Dữ liệu lưu tại trình duyệt
              </span>
            </div>
          </div>
        </header>

        <div className="grid gap-6 xl:grid-cols-[360px_1fr]">
          <section className="space-y-4 rounded-3xl bg-white border border-slate-200 p-5 shadow-sm">
            <div className="flex items-center justify-between gap-3">
              <div>
                <h2 className="text-xl font-semibold text-slate-900">Danh sách bệnh</h2>
                <p className="text-sm text-slate-500">Chọn một bệnh để xem hoặc nhấn chỉnh sửa.</p>
              </div>
              <span className="rounded-full bg-sky-100 px-3 py-1 text-sm font-semibold text-sky-700">{diseases.length} bệnh</span>
            </div>

            <div className="space-y-3">
              {diseases.length === 0 ? (
                <div className="rounded-3xl border border-dashed border-slate-300 bg-slate-50 p-6 text-center text-slate-500">
                  Hiện chưa có bệnh nào. Vui lòng thêm bệnh mới bằng form bên phải.
                </div>
              ) : (
                diseases.map((item) => (
                  <button
                    key={item.id}
                    type="button"
                    onClick={() => handleSelect(item.id)}
                    className={`w-full rounded-3xl border p-4 text-left transition ${selectedId === item.id ? 'border-sky-500 bg-sky-50 shadow-sm' : 'border-slate-200 bg-white hover:border-slate-300 hover:bg-slate-50'}`}
                  >
                    <div className="flex items-center justify-between gap-3">
                      <div>
                        <h3 className="text-base font-semibold text-slate-900">{item.diseaseName}</h3>
                        <p className="mt-1 text-sm text-slate-500 line-clamp-2">{item.description || 'Không có mô tả.'}</p>
                      </div>
                      <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold uppercase tracking-[0.15em] text-slate-700">Bệnh</span>
                    </div>
                    <div className="mt-4 flex items-center gap-2 text-sm text-slate-500">
                      <button
                        type="button"
                        onClick={() => handleEdit(item)}
                        className="font-semibold text-sky-600 hover:text-sky-800"
                      >
                        Sửa
                      </button>
                      <button
                        type="button"
                        onClick={() => handleDelete(item.id)}
                        className="font-semibold text-rose-600 hover:text-rose-800"
                      >
                        Xóa
                      </button>
                    </div>
                  </button>
                ))
              )}
            </div>
          </section>

          <div className="space-y-6">
            <section className="rounded-3xl bg-white border border-slate-200 p-6 shadow-sm">
              <h2 className="text-xl font-semibold text-slate-900">Thêm / sửa bệnh</h2>
              <form onSubmit={handleSubmit} className="mt-6 space-y-5">
                <div className="grid gap-4 lg:grid-cols-[1fr]">
                  <label className="block">
                    <span className="text-sm font-semibold text-slate-700">Tên bệnh</span>
                    <input
                      name="diseaseName"
                      value={form.diseaseName}
                      onChange={handleInputChange}
                      placeholder="Ví dụ: Cảm lạnh thông thường"
                      className="mt-2 w-full rounded-3xl border border-slate-300 bg-slate-50 px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-sky-500 focus:ring-2 focus:ring-sky-100"
                    />
                  </label>
                </div>

                <label className="block">
                  <span className="text-sm font-semibold text-slate-700">Thông tin bệnh</span>
                  <textarea
                    name="description"
                    value={form.description}
                    onChange={handleInputChange}
                    rows={4}
                    placeholder="Mô tả ngắn gọn về bệnh..."
                    className="mt-2 w-full rounded-3xl border border-slate-300 bg-slate-50 px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-sky-500 focus:ring-2 focus:ring-sky-100"
                  />
                </label>

                <label className="block">
                  <span className="text-sm font-semibold text-slate-700">Triệu chứng</span>
                  <textarea
                    name="symptoms"
                    value={form.symptoms}
                    onChange={handleInputChange}
                    rows={4}
                    placeholder="Nhập mỗi triệu chứng trên một dòng"
                    className="mt-2 w-full rounded-3xl border border-slate-300 bg-slate-50 px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-sky-500 focus:ring-2 focus:ring-sky-100"
                  />
                </label>

                <label className="block">
                  <span className="text-sm font-semibold text-slate-700">Thuốc khuyến nghị</span>
                  <textarea
                    name="recommendedMedications"
                    value={form.recommendedMedications}
                    onChange={handleInputChange}
                    rows={4}
                    placeholder="Nhập mỗi thuốc trên một dòng"
                    className="mt-2 w-full rounded-3xl border border-slate-300 bg-slate-50 px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-sky-500 focus:ring-2 focus:ring-sky-100"
                  />
                </label>

                <label className="block">
                  <span className="text-sm font-semibold text-slate-700">Hướng dẫn điều trị</span>
                  <textarea
                    name="treatmentGuidelines"
                    value={form.treatmentGuidelines}
                    onChange={handleInputChange}
                    rows={5}
                    placeholder="Ghi chú các bước điều trị và chăm sóc"
                    className="mt-2 w-full rounded-3xl border border-slate-300 bg-slate-50 px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-sky-500 focus:ring-2 focus:ring-sky-100"
                  />
                </label>

                <label className="block">
                  <span className="text-sm font-semibold text-slate-700">Cách phòng ngừa</span>
                  <textarea
                    name="prevention"
                    value={form.prevention}
                    onChange={handleInputChange}
                    rows={4}
                    placeholder="Ghi lại các biện pháp phòng tránh"
                    className="mt-2 w-full rounded-3xl border border-slate-300 bg-slate-50 px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-sky-500 focus:ring-2 focus:ring-sky-100"
                  />
                </label>

                <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <div className="text-sm text-slate-500">Bạn có thể thêm dữ liệu mới mà không cần mẫu sẵn.</div>
                  <div className="flex flex-wrap gap-3">
                    <button
                      type="submit"
                      className="rounded-3xl bg-sky-600 px-6 py-3 text-sm font-semibold text-white shadow-sm transition hover:bg-sky-700"
                    >
                      {editingId ? 'Cập nhật bệnh' : 'Thêm bệnh mới'}
                    </button>
                    <button
                      type="button"
                      onClick={resetForm}
                      className="rounded-3xl border border-slate-300 bg-white px-6 py-3 text-sm font-semibold text-slate-700 transition hover:border-slate-400 hover:bg-slate-50"
                    >
                      Làm mới form
                    </button>
                  </div>
                </div>
              </form>
            </section>

            <section className="rounded-3xl bg-white border border-slate-200 p-6 shadow-sm">
              <div className="mb-5 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <div>
                  <h2 className="text-xl font-semibold text-slate-900">Xem trước chi tiết</h2>
                  <p className="text-sm text-slate-500">Thông tin được hiển thị giống trang bệnh lý.</p>
                </div>
                <span className="rounded-full bg-slate-100 px-3 py-1 text-sm font-semibold text-slate-700">{selectedDisease ? 'Đã chọn bệnh' : 'Chưa chọn bệnh'}</span>
              </div>

              {selectedDisease ? (
                <div className="space-y-6">
                  <div className="rounded-3xl border border-slate-200 bg-slate-50 p-6">
                    <h3 className="text-2xl font-semibold text-slate-900">Chi tiết bệnh: {selectedDisease.diseaseName}</h3>
                    <p className="mt-3 text-sm leading-7 text-slate-700">{selectedDisease.description || 'Không có mô tả cho bệnh này.'}</p>
                  </div>

                  <div className="grid gap-6 lg:grid-cols-2">
                    <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
                      <h4 className="text-lg font-semibold text-slate-900">Triệu chứng</h4>
                      <ul className="mt-4 space-y-3 text-sm text-slate-700">
                        {parseList(selectedDisease.symptoms).length > 0 ? (
                          parseList(selectedDisease.symptoms).map((symptom, index) => (
                            <li key={index} className="rounded-2xl bg-slate-50 p-3">{symptom}</li>
                          ))
                        ) : (
                          <li className="text-slate-500">Chưa có triệu chứng.</li>
                        )}
                      </ul>
                    </div>

                    <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
                      <h4 className="text-lg font-semibold text-slate-900">Thuốc khuyến nghị</h4>
                      <ul className="mt-4 space-y-3 text-sm text-slate-700">
                        {parseList(selectedDisease.recommendedMedications).length > 0 ? (
                          parseList(selectedDisease.recommendedMedications).map((med, index) => (
                            <li key={index} className="rounded-2xl bg-slate-50 p-3">{med}</li>
                          ))
                        ) : (
                          <li className="text-slate-500">Chưa có thuốc khuyến nghị.</li>
                        )}
                      </ul>
                    </div>
                  </div>

                  <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
                    <h4 className="text-lg font-semibold text-slate-900">Hướng dẫn điều trị</h4>
                    <p className="mt-4 text-sm leading-7 text-slate-700">{selectedDisease.treatmentGuidelines || 'Chưa có hướng dẫn điều trị.'}</p>
                  </div>

                  <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
                    <h4 className="text-lg font-semibold text-slate-900">Cách phòng ngừa</h4>
                    <p className="mt-4 text-sm leading-7 text-slate-700">{selectedDisease.prevention || 'Chưa có nội dung phòng ngừa.'}</p>
                  </div>
                </div>
              ) : (
                <div className="rounded-3xl border border-dashed border-slate-300 bg-slate-50 p-8 text-center text-slate-500">
                  Vui lòng chọn bệnh trong danh sách để xem chi tiết.
                </div>
              )}
            </section>
          </div>
        </div>
      </div>
    </div>
  );
}

export default DiseaseManagement;
