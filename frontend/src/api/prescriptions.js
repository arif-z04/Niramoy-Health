import { apiClient } from './client.js';

export const prescriptionsApi = {
  async uploadPrescription(formData) {
    const res = await apiClient('/prescriptions/upload', {
      method: 'POST',
      body: formData,
    });
    return res.data || res;
  },

  async getMyPrescriptions(page = 1, pageSize = 20) {
    const res = await apiClient(`/prescriptions?pageNumber=${page}&pageSize=${pageSize}`);
    return res.data || res;
  },

  async getPrescriptionById(id) {
    const res = await apiClient(`/prescriptions/${id}`);
    return res.data || res;
  },

  async reviewPrescription(id, newStatus, clinicalNotes) {
    const res = await apiClient(`/prescriptions/${id}/review`, {
      method: 'POST',
      body: JSON.stringify({
        newStatus,
        decisionNotes: clinicalNotes,
        clinicalNotes: clinicalNotes,
      }),
    });
    return res.data || res;
  },
};
