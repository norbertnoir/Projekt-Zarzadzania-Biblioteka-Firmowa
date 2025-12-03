"use client"

import React, { createContext, useContext, useState, useEffect } from 'react'
import { authApi, UserDto } from '@/lib/api/auth'

interface AuthContextType {
  user: UserDto | null
  loading: boolean
  isAuthenticated: boolean
  isAdmin: boolean
  login: (token: string) => Promise<void>
  logout: () => void
  refreshUser: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(null)
  const [loading, setLoading] = useState(true)

  const loadUser = async () => {
    try {
      const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null
      if (token) {
        const userData = await authApi.me()
        setUser(userData)
      } else {
        setUser(null)
      }
    } catch (error) {
      console.error('Failed to load user:', error)
      setUser(null)
      if (typeof window !== 'undefined') {
        localStorage.removeItem('token')
      }
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadUser()
  }, [])

  const login = async (token: string) => {
    localStorage.setItem('token', token)
    await loadUser()
  }

  const logout = () => {
    localStorage.removeItem('token')
    setUser(null)
  }

  const refreshUser = async () => {
    await loadUser()
  }

  const isAuthenticated = !!user
  // Sprawdzamy czy użytkownik ma rolę administratora
  const isAdmin = user?.role === 'Admin' || user?.role === 'Administrator' || user?.role === 'admin'

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        isAuthenticated,
        isAdmin,
        login,
        logout,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
