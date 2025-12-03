"use client"

import { useEffect, useState } from "react"

type Theme = "light" | "dark" | "high-contrast"

export function useTheme() {
  const [theme, setThemeState] = useState<Theme>("light")
  const [mounted, setMounted] = useState(false)

  useEffect(() => {
    setMounted(true)
    // Sprawdź zapisany motyw lub użyj domyślnego
    const savedTheme = localStorage.getItem("theme") as Theme | null
    if (savedTheme) {
      setThemeState(savedTheme)
      applyTheme(savedTheme)
    } else {
      // Domyślnie light
      applyTheme("light")
    }
  }, [])

  const applyTheme = (newTheme: Theme) => {
    const root = document.documentElement
    root.classList.remove("dark", "high-contrast")
    
    if (newTheme === "dark") {
      root.classList.add("dark")
    } else if (newTheme === "high-contrast") {
      root.classList.add("high-contrast")
    }
  }

  const setTheme = (newTheme: Theme) => {
    setThemeState(newTheme)
    localStorage.setItem("theme", newTheme)
    applyTheme(newTheme)
  }

  return { theme, setTheme, mounted }
}

