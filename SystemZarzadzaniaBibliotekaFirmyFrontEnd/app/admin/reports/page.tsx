"use client"

import { useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import { Navigation } from "@/components/navigation"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { FileDown, FileText, Activity, RefreshCw, AlertCircle } from "lucide-react"
import api from "@/lib/api"
import { useAuth } from "@/contexts/AuthContext"
import { useToast } from "@/components/ui/use-toast"

export default function AdminReportsPage() {
    const { user, isAdmin, isAuthenticated, loading } = useAuth()
    const router = useRouter()
    const { toast } = useToast()
    const [logs, setLogs] = useState<string[]>([])
    const [logsLoading, setLogsLoading] = useState(false)

    useEffect(() => {
        if (!loading && !isAuthenticated) {
            router.push("/login")
        } else if (!loading && !isAdmin) {
            router.push("/")
        }
    }, [loading, isAuthenticated, isAdmin, router])

    // Funkcja do pobierania logów systemowych
    const fetchLogs = async () => {
        setLogsLoading(true)
        try {
            const response = await api.get<string[]>("/api/Logs")
            setLogs(response.data)
        } catch (error) {
            console.error("Failed to fetch logs", error)
            toast({
                title: "Błąd",
                description: "Nie udało się pobrać logów systemowych.",
                variant: "destructive",
            })
        } finally {
            setLogsLoading(false)
        }
    }

    useEffect(() => {
        if (isAdmin) {
            fetchLogs()
        }
    }, [isAdmin])

    // Funkcja do pobierania raportów (CSV lub PDF)
    const downloadReport = async (type: 'books' | 'loans', format: 'csv' | 'pdf') => {
        try {
            // Wybór odpowiedniego endpointu w zależności od formatu
            const endpoint = format === 'pdf'
                ? `/api/Reports/export/${type}/pdf`
                : `/api/Reports/export/${type}`

            // Pobranie pliku jako blob (binary large object)
            const response = await api.get(endpoint, {
                responseType: 'blob',
            })

            // Utworzenie tymczasowego linku do pobrania pliku
            const url = window.URL.createObjectURL(new Blob([response.data]))
            const link = document.createElement('a')
            link.href = url
            const date = new Date().toISOString().split('T')[0]
            link.setAttribute('download', `raport_${type}_${date}.${format}`)
            document.body.appendChild(link)
            link.click()
            link.remove()

            toast({
                title: "Sukces",
                description: `Raport ${type === 'books' ? 'książek' : 'wypożyczeń'} (${format.toUpperCase()}) został pobrany.`,
            })
        } catch (error) {
            console.error("Failed to download report", error)
            toast({
                title: "Błąd",
                description: "Nie udało się wygenerować raportu.",
                variant: "destructive",
            })
        }
    }

    if (loading || !isAdmin) {
        return null
    }

    return (
        <div className="min-h-screen bg-background">
            <Navigation />
            <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
                <div className="mb-8">
                    <h1 className="text-3xl font-bold text-foreground">Panel Administratora</h1>
                    <p className="mt-2 text-muted-foreground">
                        Raporty i logi systemowe
                    </p>
                </div>

                <div className="grid gap-6 md:grid-cols-2 mb-8">
                    {/* Reports Section */}
                    <Card>
                        <CardHeader>
                            <div className="flex items-center gap-2">
                                <FileText className="h-5 w-5 text-primary" />
                                <CardTitle>Generowanie Raportów</CardTitle>
                            </div>
                            <CardDescription>
                                Pobierz dane systemowe w formacie CSV lub PDF
                            </CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="flex items-center justify-between p-4 border rounded-lg bg-card">
                                <div>
                                    <h3 className="font-medium">Raport Książek</h3>
                                    <p className="text-sm text-muted-foreground">Pełna lista książek z autorami i statusem</p>
                                </div>
                                <div className="flex gap-2">
                                    <Button onClick={() => downloadReport('books', 'csv')} variant="outline" className="gap-2">
                                        <FileDown className="h-4 w-4" />
                                        CSV
                                    </Button>
                                    <Button onClick={() => downloadReport('books', 'pdf')} variant="outline" className="gap-2">
                                        <FileDown className="h-4 w-4" />
                                        PDF
                                    </Button>
                                </div>
                            </div>

                            <div className="flex items-center justify-between p-4 border rounded-lg bg-card">
                                <div>
                                    <h3 className="font-medium">Raport Wypożyczeń</h3>
                                    <p className="text-sm text-muted-foreground">Historia wypożyczeń i obecne stany</p>
                                </div>
                                <div className="flex gap-2">
                                    <Button onClick={() => downloadReport('loans', 'csv')} variant="outline" className="gap-2">
                                        <FileDown className="h-4 w-4" />
                                        CSV
                                    </Button>
                                    <Button onClick={() => downloadReport('loans', 'pdf')} variant="outline" className="gap-2">
                                        <FileDown className="h-4 w-4" />
                                        PDF
                                    </Button>
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    {/* System Status Section (Placeholder for future expansion) */}
                    <Card>
                        <CardHeader>
                            <div className="flex items-center gap-2">
                                <Activity className="h-5 w-5 text-primary" />
                                <CardTitle>Status Systemu</CardTitle>
                            </div>
                            <CardDescription>
                                Podstawowe informacje o działaniu aplikacji
                            </CardDescription>
                        </CardHeader>
                        <CardContent>
                            <div className="space-y-4">
                                <div className="flex items-center gap-2 text-sm">
                                    <div className="h-2 w-2 rounded-full bg-green-500" />
                                    <span>System działa poprawnie</span>
                                </div>
                                <div className="flex items-center gap-2 text-sm">
                                    <div className="h-2 w-2 rounded-full bg-green-500" />
                                    <span>Baza danych połączona</span>
                                </div>
                                <div className="flex items-center gap-2 text-sm">
                                    <div className="h-2 w-2 rounded-full bg-green-500" />
                                    <span>Logowanie plików aktywne</span>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                </div>

                {/* Logs Section */}
                <Card>
                    <CardHeader>
                        <div className="flex items-center justify-between">
                            <div className="flex items-center gap-2">
                                <AlertCircle className="h-5 w-5 text-primary" />
                                <div>
                                    <CardTitle>Logi Systemowe</CardTitle>
                                    <CardDescription>Ostatnie 100 wpisów z logów aplikacji</CardDescription>
                                </div>
                            </div>
                            <Button onClick={fetchLogs} variant="ghost" size="sm" disabled={logsLoading}>
                                <RefreshCw className={`h-4 w-4 ${logsLoading ? 'animate-spin' : ''}`} />
                            </Button>
                        </div>
                    </CardHeader>
                    <CardContent>
                        <div className="bg-muted/50 rounded-lg p-4 font-mono text-xs h-[400px] overflow-y-auto whitespace-pre-wrap border">
                            {logsLoading ? (
                                <div className="flex items-center justify-center h-full text-muted-foreground">
                                    Ładowanie logów...
                                </div>
                            ) : logs.length > 0 ? (
                                logs.map((line, index) => (
                                    <div key={index} className="mb-1 border-b border-border/50 pb-1 last:border-0">
                                        {line}
                                    </div>
                                ))
                            ) : (
                                <div className="flex items-center justify-center h-full text-muted-foreground">
                                    Brak logów do wyświetlenia
                                </div>
                            )}
                        </div>
                    </CardContent>
                </Card>
            </main>
        </div>
    )
}
