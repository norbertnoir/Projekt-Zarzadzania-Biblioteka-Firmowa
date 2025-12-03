import apiClient from '../api'
import { Book, CreateBookDto, UpdateBookDto, ArrayResponse } from '@/types'

export const booksApi = {
  getAll: async (): Promise<Book[]> => {
    const response = await apiClient.get<ArrayResponse<Book>>('/api/Books')
    return response.data
  },

  getById: async (id: number): Promise<Book> => {
    const response = await apiClient.get<Book>(`/api/Books/${id}`)
    return response.data
  },

  create: async (data: CreateBookDto): Promise<Book> => {
    const response = await apiClient.post<Book>('/api/Books', data)
    return response.data
  },

  update: async (id: number, data: UpdateBookDto): Promise<Book> => {
    const response = await apiClient.put<Book>(`/api/Books/${id}`, data)
    return response.data
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/Books/${id}`)
  },

  deleteBulk: async (ids: number[]): Promise<{ message: string; deletedCount: number; requestedCount: number }> => {
    const response = await apiClient.delete<{ message: string; deletedCount: number; requestedCount: number }>('/api/Books/bulk', {
      data: ids
    })
    return response.data
  },

  search: async (term: string): Promise<Book[]> => {
    const response = await apiClient.get<ArrayResponse<Book>>(`/api/Books/search?term=${encodeURIComponent(term)}`)
    return response.data
  },

  getAvailable: async (): Promise<Book[]> => {
    const response = await apiClient.get<ArrayResponse<Book>>('/api/Books/available')
    return response.data
  },

  getByCategory: async (categoryId: number): Promise<Book[]> => {
    const response = await apiClient.get<ArrayResponse<Book>>(`/api/Books/category/${categoryId}`)
    return response.data
  },
}
