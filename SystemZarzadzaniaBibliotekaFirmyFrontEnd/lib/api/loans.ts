import apiClient from '../api'
import { Loan, CreateLoanDto, UpdateLoanDto, ReturnLoanDto, ArrayResponse } from '@/types'

export const loansApi = {
  getAll: async (): Promise<Loan[]> => {
    const response = await apiClient.get<ArrayResponse<Loan>>('/api/Loans')
    return response.data
  },

  getById: async (id: number): Promise<Loan> => {
    const response = await apiClient.get<Loan>(`/api/Loans/${id}`)
    return response.data
  },

  create: async (data: CreateLoanDto): Promise<Loan> => {
    const response = await apiClient.post<Loan>('/api/Loans', data)
    return response.data
  },

  update: async (id: number, data: UpdateLoanDto): Promise<Loan> => {
    const response = await apiClient.put<Loan>(`/api/Loans/${id}`, data)
    return response.data
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/Loans/${id}`)
  },

  return: async (id: number, data: ReturnLoanDto): Promise<Loan> => {
    const response = await apiClient.post<Loan>(`/api/Loans/${id}/return`, data)
    return response.data
  },

  getByEmployee: async (employeeId: number): Promise<Loan[]> => {
    const response = await apiClient.get<ArrayResponse<Loan>>(`/api/Loans/employee/${employeeId}`)
    return response.data
  },

  getActive: async (): Promise<Loan[]> => {
    const response = await apiClient.get<ArrayResponse<Loan>>('/api/Loans/active')
    return response.data
  },

  getOverdue: async (): Promise<Loan[]> => {
    const response = await apiClient.get<ArrayResponse<Loan>>('/api/Loans/overdue')
    return response.data
  },
}
