"use client"

import { useState, useEffect } from "react"
import { Navigation } from "@/components/navigation"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { loansApi } from "@/lib/api/loans"
import { booksApi } from "@/lib/api/books"
import { employeesApi } from "@/lib/api/employees"
import { Loan, CreateLoanDto, UpdateLoanDto, Book, Employee } from "@/types"
import { Plus, Calendar, Edit, Trash2 } from "lucide-react"
import { useToast } from "@/hooks/use-toast"
import { useAuth } from "@/contexts/AuthContext"
import { format } from "date-fns"
import { pl } from "date-fns/locale/pl"
import Link from "next/link"

export default function LoansPage() {
  const { isAuthenticated, isAdmin } = useAuth()
  const [loans, setLoans] = useState<Loan[]>([])
  const [books, setBooks] = useState<Book[]>([])
  const [employees, setEmployees] = useState<Employee[]>([])
  const [loading, setLoading] = useState(true)
  const [isDialogOpen, setIsDialogOpen] = useState(false)
  const [editingLoan, setEditingLoan] = useState<Loan | null>(null)
  const { toast } = useToast()

  const [formData, setFormData] = useState<CreateLoanDto>({
    bookId: 0,
    employeeId: 0,
    dueDate: "",
    notes: "",
  })

  useEffect(() => {
    loadLoans()
    loadBooks()
    loadEmployees()
  }, [])

  const loadLoans = async () => {
    try {
      setLoading(true)
      const data = await loansApi.getAll()
      setLoans(data)
    } catch (error) {
      toast({
        title: "Błąd",
        description: "Nie udało się załadować wypożyczeń",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  const loadBooks = async () => {
    try {
      // Dla edycji potrzebujemy wszystkich książek, nie tylko dostępnych
      const data = await booksApi.getAll()
      setBooks(data)
    } catch (error) {
      // Silently fail
    }
  }

  const loadEmployees = async () => {
    try {
      const data = await employeesApi.getAll()
      setEmployees(data)
    } catch (error) {
      // Silently fail
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editingLoan) {
        await loansApi.update(editingLoan.id, formData as UpdateLoanDto)
        toast({
          title: "Sukces",
          description: "Wypożyczenie zostało zaktualizowane",
        })
      } else {
        await loansApi.create(formData)
        toast({
          title: "Sukces",
          description: "Wypożyczenie zostało utworzone",
        })
      }
      setIsDialogOpen(false)
      resetForm()
      loadLoans()
      loadBooks()
    } catch (error: any) {
      toast({
        title: "Błąd",
        description: error.response?.data?.message || "Wystąpił błąd",
        variant: "destructive",
      })
    }
  }

  const handleEdit = (loan: Loan) => {
    setEditingLoan(loan)
    setFormData({
      bookId: loan.bookId,
      employeeId: loan.employeeId,
      dueDate: loan.dueDate.split('T')[0], // Format dla input type="date"
      notes: loan.notes || "",
    })
    setIsDialogOpen(true)
  }

  const handleDelete = async (id: number) => {
    if (!confirm("Czy na pewno chcesz usunąć to wypożyczenie?")) return

    try {
      await loansApi.delete(id)
      toast({
        title: "Sukces",
        description: "Wypożyczenie zostało usunięte",
      })
      loadLoans()
      loadBooks()
    } catch (error: any) {
      toast({
        title: "Błąd",
        description: error.response?.data?.message || "Wystąpił błąd",
        variant: "destructive",
      })
    }
  }

  const resetForm = () => {
    setFormData({
      bookId: 0,
      employeeId: 0,
      dueDate: "",
      notes: "",
    })
    setEditingLoan(null)
  }

  const handleReturn = async (loanId: number) => {
    try {
      await loansApi.return(loanId, {
        returnDate: new Date().toISOString(),
        notes: "",
      })
      toast({
        title: "Sukces",
        description: "Książka została zwrócona",
      })
      loadLoans()
      loadBooks()
    } catch (error: any) {
      toast({
        title: "Błąd",
        description: error.response?.data?.message || "Wystąpił błąd",
        variant: "destructive",
      })
    }
  }

  return (
    <div className="min-h-screen bg-background">
      <Navigation />
      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="mb-6 flex items-center justify-between">
          <h1 className="text-3xl font-bold text-foreground">Wypożyczenia</h1>
          {isAuthenticated && (
            <Dialog open={isDialogOpen} onOpenChange={(open) => {
              setIsDialogOpen(open)
              if (!open) resetForm()
            }}>
              <DialogTrigger asChild>
                <Button onClick={() => resetForm()}>
                  <Plus className="mr-2 h-4 w-4" />
                  Nowe wypożyczenie
                </Button>
              </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>
                  {editingLoan ? "Edytuj wypożyczenie" : "Utwórz nowe wypożyczenie"}
                </DialogTitle>
                <DialogDescription>
                  {editingLoan
                    ? "Zaktualizuj dane wypożyczenia"
                    : "Wybierz książkę i użytkownika, aby utworzyć wypożyczenie"}
                </DialogDescription>
              </DialogHeader>
              <form onSubmit={handleSubmit}>
                <div className="grid gap-4 py-4">
                  <div className="space-y-2">
                    <Label htmlFor="bookId">Książka *</Label>
                    <Select
                      value={formData.bookId.toString()}
                      onValueChange={(value) =>
                        setFormData({ ...formData, bookId: parseInt(value) })
                      }
                      disabled={!!editingLoan}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Wybierz książkę" />
                      </SelectTrigger>
                      <SelectContent>
                        {books.map((book) => (
                          <SelectItem key={book.id} value={book.id.toString()}>
                            {book.title} {book.authors?.length ? `- ${book.authors.map(a => a.fullName || `${a.firstName} ${a.lastName}`).join(", ")}` : ""}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="employeeId">Pracownik *</Label>
                    <Select
                      value={formData.employeeId.toString()}
                      onValueChange={(value) =>
                        setFormData({ ...formData, employeeId: parseInt(value) })
                      }
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Wybierz pracownika" />
                      </SelectTrigger>
                      <SelectContent>
                        {employees.map((employee) => (
                          <SelectItem key={employee.id} value={employee.id.toString()}>
                            {employee.fullName || `${employee.firstName} ${employee.lastName}`} ({employee.email})
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="dueDate">Termin zwrotu *</Label>
                    <Input
                      id="dueDate"
                      type="date"
                      value={formData.dueDate}
                      onChange={(e) =>
                        setFormData({ ...formData, dueDate: e.target.value })
                      }
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="notes">Notatki</Label>
                    <Input
                      id="notes"
                      type="text"
                      value={formData.notes || ""}
                      onChange={(e) =>
                        setFormData({ ...formData, notes: e.target.value })
                      }
                      placeholder="Opcjonalne notatki"
                    />
                  </div>
                </div>
                <DialogFooter>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => setIsDialogOpen(false)}
                  >
                    Anuluj
                  </Button>
                  <Button type="submit">
                    {editingLoan ? "Zaktualizuj" : "Utwórz"}
                  </Button>
                </DialogFooter>
              </form>
            </DialogContent>
          </Dialog>
          )}
        </div>

        <div className="rounded-md border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Książka</TableHead>
                <TableHead>Pracownik</TableHead>
                <TableHead>Data wypożyczenia</TableHead>
                <TableHead>Termin zwrotu</TableHead>
                <TableHead>Data zwrotu</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Akcje</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center">
                    Ładowanie...
                  </TableCell>
                </TableRow>
              ) : loans.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center">
                    Brak wypożyczeń
                  </TableCell>
                </TableRow>
              ) : (
                loans.map((loan) => (
                  <TableRow key={loan.id}>
                    <TableCell className="font-medium">
                      <Link
                        href={`/books/${loan.bookId}`}
                        className="text-primary hover:underline"
                      >
                        {loan.bookTitle || `Książka #${loan.bookId}`}
                      </Link>
                    </TableCell>
                    <TableCell>
                      {loan.employeeName || `Pracownik #${loan.employeeId}`}
                    </TableCell>
                    <TableCell>
                      {format(new Date(loan.loanDate), "dd MMM yyyy", {
                        locale: pl,
                      })}
                    </TableCell>
                    <TableCell>
                      {format(new Date(loan.dueDate), "dd MMM yyyy", {
                        locale: pl,
                      })}
                    </TableCell>
                    <TableCell>
                      {loan.returnDate
                        ? format(new Date(loan.returnDate), "dd MMM yyyy", {
                            locale: pl,
                          })
                        : "-"}
                    </TableCell>
                    <TableCell>
                      <span
                        className={`inline-flex rounded-full px-2 py-1 text-xs font-semibold ${
                          loan.isReturned
                            ? "bg-muted text-muted-foreground high-contrast:bg-transparent high-contrast:text-foreground"
                            : new Date(loan.dueDate) < new Date() && !loan.isReturned
                            ? "bg-red-500/10 text-red-700 dark:text-red-400 high-contrast:bg-transparent high-contrast:text-foreground"
                            : "bg-blue-500/10 text-blue-700 dark:text-blue-400 high-contrast:bg-transparent high-contrast:text-foreground"
                        }`}
                      >
                        {loan.isReturned
                          ? "Zwrócone"
                          : new Date(loan.dueDate) < new Date()
                          ? "Przeterminowane"
                          : "Aktywne"}
                      </span>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-2">
                        {isAdmin && (
                          <>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleEdit(loan)}
                            >
                              <Edit className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleDelete(loan.id)}
                            >
                              <Trash2 className="h-4 w-4" />
                            </Button>
                          </>
                        )}
                        {!loan.isReturned && isAuthenticated && (
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleReturn(loan.id)}
                          >
                            Zwróć
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>

      </main>
    </div>
  )
}

