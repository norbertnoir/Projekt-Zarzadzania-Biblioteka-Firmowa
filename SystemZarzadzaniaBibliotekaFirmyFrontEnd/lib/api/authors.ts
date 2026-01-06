
import axios from 'axios';
import { Author, CreateAuthorDto } from '@/types';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5205';

const api = axios.create({
    baseURL: `${API_URL}/api`,
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export const authorsApi = {
    getAll: async (): Promise<Author[]> => {
        const response = await api.get<Author[]>('/authors');
        return response.data;
    },

    getById: async (id: number): Promise<Author> => {
        const response = await api.get<Author>(`/authors/${id}`);
        return response.data;
    },

    create: async (data: CreateAuthorDto): Promise<Author> => {
        const response = await api.post<Author>('/authors', data);
        return response.data;
    },
};
