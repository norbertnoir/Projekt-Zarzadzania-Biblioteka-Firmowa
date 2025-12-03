import apiClient from '../api'
import { Employee, CreateEmployeeDto, UpdateEmployeeDto, ArrayResponse } from '@/types'

export const employeesApi = {
  getAll: async (): Promise<Employee[]> => {
    const response = await apiClient.get<ArrayResponse<Employee>>('/api/Employees')
    return response.data
  },

  getById: async (id: number): Promise<Employee> => {
    const response = await apiClient.get<Employee>(`/api/Employees/${id}`)
    return response.data
  },

  create: async (data: CreateEmployeeDto): Promise<Employee> => {
    const response = await apiClient.post<Employee>('/api/Employees', data)
    return response.data
  },

  update: async (id: number, data: UpdateEmployeeDto): Promise<Employee> => {
    const response = await apiClient.put<Employee>(`/api/Employees/${id}`, data)
    return response.data
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/Employees/${id}`)
  },
}

