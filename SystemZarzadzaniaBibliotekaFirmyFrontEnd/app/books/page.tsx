"use client"

import { useState, useEffect, useCallback } from "react"
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
import { Checkbox } from "@/components/ui/checkbox"
import { booksApi } from "@/lib/api/books"
import { loansApi } from "@/lib/api/loans"
import { Book, CreateBookDto, CreateLoanDto, Author } from "@/types"
import { Plus, Search, Edit, Trash2, Eye, BookOpen } from "lucide-react"
import { useToast } from "@/hooks/use-toast"
import { useAuth } from "@/contexts/AuthContext"
import { authorsApi } from "@/lib/api/authors"
import { AddAuthorDialog } from "@/components/add-author-dialog"
import Link from "next/link"

export default function BooksPage() {
  const { isAdmin, user, loading: authLoading } = useAuth()
  const [books, setBooks] = useState<Book[]>([])
  const [authors, setAuthors] = useState<Author[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState("")
  const [isDialogOpen, setIsDialogOpen] = useState(false)
  const [isBulkDeleteDialogOpen, setIsBulkDeleteDialogOpen] = useState(false)
  const [editingBook, setEditingBook] = useState<Book | null>(null)
  const [selectedBooks, setSelectedBooks] = useState<number[]>([])
  const [errors, setErrors] = useState<Record<string, string>>({})
  const { toast } = useToast()

  const [formData, setFormData] = useState<CreateBookDto>({
    title: "",
    isbn: "",
    year: new Date().getFullYear(),
    pages: 0,
    publisher: "",
    description: "",
    categoryId: 0,
    authorIds: [],
  })

  const loadBooks = useCallback(async () => {
    try {
      setLoading(true)
      const [booksData, authorsData] = await Promise.all([
        booksApi.getAll(),
        authorsApi.getAll()
      ])
      setBooks(booksData)
      setAuthors(authorsData)
    } catch (error) {
      toast({
        title: "Błąd",
        description: "Nie udało się załadować danych",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }, [toast])

  const handleSearch = useCallback(async () => {
    if (!searchTerm) {
      loadBooks()
      return
    }
    try {
      setLoading(true)
      const data = await booksApi.search(searchTerm)
      setBooks(data)
    } catch (error) {
      toast({
        title: "Błąd",
        description: "Nie udało się wyszukać książek",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }, [searchTerm, loadBooks, toast])

  useEffect(() => {
    loadBooks()
  }, [loadBooks])

  useEffect(() => {
    if (searchTerm) {
      handleSearch()
    } else {
      loadBooks()
    }
  }, [searchTerm, handleSearch, loadBooks])

  const validateForm = () => {
    const newErrors: Record<string, string> = {}

    if (!formData.title?.trim()) newErrors.title = "Tytuł jest wymagany"
    if (!formData.isbn?.trim()) {
      newErrors.isbn = "ISBN jest wymagany"
    } else if (!/^(?:\d{10}|\d{13})$/.test(formData.isbn.trim())) {
      newErrors.isbn = "ISBN musi mieć 10 lub 13 cyfr"
    }
    if (formData.year < 1000 || formData.year > 2100) newErrors.year = "Rok musi być między 1000 a 2100"
    if (formData.pages < 1 || formData.pages > 10000) newErrors.pages = "Liczba stron musi być między 1 a 10000"
    if (formData.categoryId < 1) newErrors.categoryId = "Kategoria jest wymagana"
    if (!formData.publisher?.trim()) newErrors.publisher = "Wydawca jest wymagany"
    if (!formData.authorIds || formData.authorIds.length === 0) newErrors.authorIds = "Przynajmniej jeden autor jest wymagany"

    setErrors(newErrors)
    return newErrors
  }

  // Funkcja obsługująca wysyłanie formularza (dodawanie/edycja)
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    const validationErrors = validateForm()
    if (Object.keys(validationErrors).length > 0) {
      toast({
        title: "Błąd walidacji",
        description: Object.values(validationErrors).join("\n"),
        variant: "destructive",
      })
      return
    }

    try {
      if (editingBook) {
        // Aktualizacja istniejącej książki
        await booksApi.update(editingBook.id, formData)
        toast({
          title: "Sukces",
          description: "Książka została zaktualizowana",
        })
      } else {
        // Dodanie nowej książki
        await booksApi.create(formData)
        toast({
          title: "Sukces",
          description: "Książka została dodana",
        })
      }
      setIsDialogOpen(false)
      resetForm()
      loadBooks()
    } catch (error: any) {
      const responseData = error.response?.data
      let errorMessage = responseData?.message || "Wystąpił błąd"

      // Obsługa tablicy błędów z backendu
      if (responseData?.errors && Array.isArray(responseData.errors)) {
        errorMessage = responseData.errors.join("\n")
      }

      toast({
        title: "Błąd",
        description: errorMessage,
        variant: "destructive",
      })
    }
  }

  // Funkcja usuwająca książkę
  const handleDelete = async (id: number) => {
    if (!confirm("Czy na pewno chcesz usunąć tę książkę?")) return

    try {
      await booksApi.delete(id)
      toast({
        title: "Sukces",
        description: "Książka została usunięta",
      })
      loadBooks()
    } catch (error: any) {
      toast({
        title: "Błąd",
        description: error.response?.data?.message || "Wystąpił błąd",
        variant: "destructive",
      })
    }
  }

  const handleBorrow = async (book: Book) => {
    if (!user?.employeeId && !isAdmin) {
      toast({
        title: "Błąd",
        description: "Nie jesteś powiązany z żadnym pracownikiem. Skontaktuj się z administratorem.",
        variant: "destructive",
      })
      return
    }

    if (!confirm(`Czy na pewno chcesz wypożyczyć książkę "${book.title}"?`)) return

    try {
      const loanData: CreateLoanDto = {
        bookId: book.id,
        employeeId: user?.employeeId || 0, // Backend should handle 0/null for non-admins or we handle it there
        dueDate: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString(), // 14 days from now
        notes: "Wypożyczenie własne przez stronę"
      }

      await loansApi.create(loanData)
      toast({
        title: "Sukces",
        description: "Książka została wypożyczona",
      })
      loadBooks()
    } catch (error: any) {
      toast({
        title: "Błąd",
        description: error.response?.data?.message || "Wystąpił błąd podczas wypożyczania",
        variant: "destructive",
      })
    }
  }

  const handleEdit = (book: Book) => {
    setEditingBook(book)
    setFormData({
      title: book.title || "",
      isbn: book.isbn || "",
      year: book.year,
      pages: book.pages,
      publisher: book.publisher || "",
      description: book.description || "",
      categoryId: book.categoryId,
      authorIds: book.authors?.map((a) => a.id) || [],
    })
    setIsDialogOpen(true)
  }

  const resetForm = () => {
    setFormData({
      title: "",
      isbn: "",
      year: new Date().getFullYear(),
      pages: 0,
      publisher: "",
      description: "",
      categoryId: 0,
      authorIds: [],
    })
    setErrors({})
    setEditingBook(null)
  }

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      setSelectedBooks(books.map(book => book.id))
    } else {
      setSelectedBooks([])
    }
  }

  const handleSelectBook = (bookId: number, checked: boolean) => {
    if (checked) {
      setSelectedBooks([...selectedBooks, bookId])
    } else {
      setSelectedBooks(selectedBooks.filter(id => id !== bookId))
    }
  }

  const handleBulkDelete = async () => {
    if (selectedBooks.length === 0) return

    try {
      const result = await booksApi.deleteBulk(selectedBooks)

      if (result.deletedCount === result.requestedCount) {
        toast({
          title: "Sukces",
          description: "Usunięto " + result.deletedCount + " książek",
        })
      } else {
        toast({
          title: "Częściowy sukces",
          description: "Usunięto " + result.deletedCount + " z " + result.requestedCount + " książek. Niektóre książki nie zostały znalezione.",
          variant: "default",
        })
      }

      setIsBulkDeleteDialogOpen(false)
      setSelectedBooks([])
      loadBooks()
    } catch (error: any) {
      toast({
        title: "Błąd",
        description: error.response?.data?.message || "Wystąpił błąd podczas usuwania książek",
        variant: "destructive",
      })
    }
  }

  // Nie renderuj strony podczas ładowania autoryzacji
  if (authLoading) {
    return (
      <div className="min-h-screen bg-background">
        <Navigation />
        <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
          <div className="text-center">Ładowanie...</div>
        </main>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-background">
      <Navigation />
      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="mb-6 flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-foreground">Książki</h1>
            {selectedBooks.length > 0 && (
              <p className="text-sm text-muted-foreground mt-1">
                Zaznaczono: {selectedBooks.length}
              </p>
            )}
          </div>
          <div className="flex items-center gap-2">
            {isAdmin && selectedBooks.length > 0 && (
              <Dialog open={isBulkDeleteDialogOpen} onOpenChange={setIsBulkDeleteDialogOpen}>
                <DialogTrigger asChild>
                  <Button variant="destructive">
                    <Trash2 className="mr-2 h-4 w-4" />
                    Usuń zaznaczone ({selectedBooks.length})
                  </Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Usuń zaznaczone książki</DialogTitle>
                    <DialogDescription>
                      Czy na pewno chcesz usunąć {selectedBooks.length} {selectedBooks.length === 1 ? 'książkę' : 'książek'}?
                    </DialogDescription>
                  </DialogHeader>
                  <div className="max-h-60 overflow-y-auto">
                    <ul className="list-disc list-inside space-y-1">
                      {books
                        .filter(book => selectedBooks.includes(book.id))
                        .map(book => (
                          <li key={book.id} className="text-sm">
                            {book.title || `Książka #${book.id}`}
                          </li>
                        ))}
                    </ul>
                  </div>
                  <DialogFooter>
                    <Button
                      variant="outline"
                      onClick={() => setIsBulkDeleteDialogOpen(false)}
                    >
                      Anuluj
                    </Button>
                    <Button
                      variant="destructive"
                      onClick={handleBulkDelete}
                    >
                      Usuń
                    </Button>
                  </DialogFooter>
                </DialogContent>
              </Dialog>
            )}
            {isAdmin && (
              <Dialog open={isDialogOpen} onOpenChange={(open) => {
                setIsDialogOpen(open)
                if (!open) resetForm()
              }}>
                <DialogTrigger asChild>
                  <Button onClick={() => {
                    resetForm()
                    setErrors({})
                  }}>
                    <Plus className="mr-2 h-4 w-4" />
                    Dodaj książkę
                  </Button>
                </DialogTrigger>
                <DialogContent className="max-w-2xl">
                  <DialogHeader>
                    <DialogTitle>
                      {editingBook ? "Edytuj książkę" : "Dodaj nową książkę"}
                    </DialogTitle>
                    <DialogDescription>
                      {editingBook
                        ? "Zaktualizuj informacje o książce"
                        : "Wypełnij formularz, aby dodać nową książkę do biblioteki"}
                    </DialogDescription>
                  </DialogHeader>
                  <form onSubmit={handleSubmit}>
                    <div className="grid gap-4 py-4">
                      <div className="space-y-2">
                        <Label htmlFor="title">Tytuł *</Label>
                        <Input
                          id="title"
                          value={formData.title}
                          onChange={(e) =>
                            setFormData({ ...formData, title: e.target.value })
                          }
                          className={errors.title ? "border-red-500" : ""}
                          required
                        />
                      </div>
                      <div className="space-y-2">
                        <div className="flex items-center justify-between">
                          <Label>Autorzy *</Label>
                          <AddAuthorDialog onAuthorCreated={() => {
                            authorsApi.getAll().then(setAuthors)
                          }} />
                        </div>
                        <div className={`border rounded-md p-2 h-40 overflow-y-auto space-y-2 ${errors.authorIds ? "border-red-500" : ""}`}>
                          {authors.map((author) => (
                            <div key={author.id} className="flex items-center space-x-2">
                              <Checkbox
                                id={`author-${author.id}`}
                                checked={formData.authorIds?.includes(author.id)}
                                onCheckedChange={(checked) => {
                                  if (checked) {
                                    setFormData({
                                      ...formData,
                                      authorIds: [...(formData.authorIds || []), author.id],
                                    })
                                  } else {
                                    setFormData({
                                      ...formData,
                                      authorIds: formData.authorIds?.filter((id) => id !== author.id),
                                    })
                                  }
                                }}
                              />
                              <Label
                                htmlFor={`author-${author.id}`}
                                className="text-sm font-normal cursor-pointer"
                              >
                                {author.fullName || `${author.firstName} ${author.lastName}`}
                              </Label>
                            </div>
                          ))}
                          {authors.length === 0 && (
                            <p className="text-sm text-muted-foreground text-center py-4">
                              Brak autorów. Dodaj nowego autora.
                            </p>
                          )}
                        </div>
                      </div>
                      <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                          <Label htmlFor="isbn">ISBN *</Label>
                          <Input
                            id="isbn"
                            value={formData.isbn}
                            onChange={(e) =>
                              setFormData({ ...formData, isbn: e.target.value })
                            }
                            className={errors.isbn ? "border-red-500" : ""}
                          />
                        </div>
                        <div className="space-y-2">
                          <Label htmlFor="year">Rok wydania *</Label>
                          <Input
                            id="year"
                            type="number"
                            min="1000"
                            max="2100"
                            value={formData.year}
                            onChange={(e) =>
                              setFormData({
                                ...formData,
                                year: parseInt(e.target.value) || new Date().getFullYear(),
                              })
                            }
                            className={errors.year ? "border-red-500" : ""}
                            required
                          />
                        </div>
                      </div>
                      <div className="grid grid-cols-2 gap-4">
                        <div className="space-y-2">
                          <Label htmlFor="pages">Liczba stron *</Label>
                          <Input
                            id="pages"
                            type="number"
                            min="1"
                            value={formData.pages}
                            onChange={(e) =>
                              setFormData({
                                ...formData,
                                pages: parseInt(e.target.value) || 0,
                              })
                            }
                            className={errors.pages ? "border-red-500" : ""}
                            required
                          />
                        </div>
                        <div className="space-y-2">
                          <Label htmlFor="categoryId">ID Kategorii *</Label>
                          <Input
                            id="categoryId"
                            type="number"
                            min="1"
                            value={formData.categoryId}
                            onChange={(e) =>
                              setFormData({
                                ...formData,
                                categoryId: parseInt(e.target.value) || 0,
                              })
                            }
                            className={errors.categoryId ? "border-red-500" : ""}
                            required
                          />
                        </div>
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="publisher">Wydawca *</Label>
                        <Input
                          id="publisher"
                          value={formData.publisher}
                          onChange={(e) =>
                            setFormData({ ...formData, publisher: e.target.value })
                          }
                          className={errors.publisher ? "border-red-500" : ""}
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="description">Opis</Label>
                        <textarea
                          id="description"
                          className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                          value={formData.description}
                          onChange={(e) =>
                            setFormData({ ...formData, description: e.target.value })
                          }
                        />
                      </div>
                    </div>
                    <DialogFooter>
                      <Button
                        type="button"
                        variant="outline"
                        onClick={() => {
                          setIsDialogOpen(false)
                          resetForm()
                        }}
                      >
                        Anuluj
                      </Button>
                      <Button type="submit">
                        {editingBook ? "Zaktualizuj" : "Dodaj"}
                      </Button>
                    </DialogFooter>
                  </form>
                </DialogContent>
              </Dialog>
            )}
          </div>
        </div>

        <div className="mb-4">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Szukaj książek..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>

        <div className="rounded-md border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                {isAdmin && (
                  <TableHead className="w-12">
                    <Checkbox
                      checked={books.length > 0 && selectedBooks.length === books.length}
                      indeterminate={selectedBooks.length > 0 && selectedBooks.length < books.length}
                      onCheckedChange={handleSelectAll}
                    />
                  </TableHead>
                )}
                <TableHead>Tytuł</TableHead>
                <TableHead>Autor</TableHead>
                <TableHead>ISBN</TableHead>
                <TableHead>Rok wydania</TableHead>
                <TableHead>Kategoria</TableHead>
                <TableHead>Dostępność</TableHead>
                <TableHead className="text-right">Akcje</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={isAdmin ? 8 : 7} className="text-center">
                    Ładowanie...
                  </TableCell>
                </TableRow>
              ) : books.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={isAdmin ? 8 : 7} className="text-center">
                    Brak książek
                  </TableCell>
                </TableRow>
              ) : (
                books.map((book) => (
                  <TableRow key={book.id}>
                    {isAdmin && (
                      <TableCell>
                        <Checkbox
                          checked={selectedBooks.includes(book.id)}
                          onCheckedChange={(checked) => handleSelectBook(book.id, checked as boolean)}
                        />
                      </TableCell>
                    )}
                    <TableCell className="font-medium">{book.title || "-"}</TableCell>
                    <TableCell>
                      {book.authors?.map((a) => a.fullName || `${a.firstName} ${a.lastName}`).join(", ") || "-"}
                    </TableCell>
                    <TableCell>{book.isbn || "-"}</TableCell>
                    <TableCell>{book.year || "-"}</TableCell>
                    <TableCell>{book.categoryName || "-"}</TableCell>
                    <TableCell>
                      <span
                        className={`inline-flex rounded-full px-2 py-1 text-xs font-semibold ${book.isAvailable
                          ? "bg-green-500/10 text-green-700 dark:text-green-400 high-contrast:bg-transparent high-contrast:text-foreground"
                          : "bg-red-500/10 text-red-700 dark:text-red-400 high-contrast:bg-transparent high-contrast:text-foreground"
                          }`}
                      >
                        {book.isAvailable ? "Dostępna" : "Wypożyczona"}
                      </span>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end space-x-2">
                        <Link href={`/books/${book.id}`}>
                          <Button variant="ghost" size="icon" title="Szczegóły">
                            <Eye className="h-4 w-4" />
                          </Button>
                        </Link>
                        {!isAdmin && book.isAvailable && (
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => handleBorrow(book)}
                            title="Wypożycz"
                          >
                            <BookOpen className="h-4 w-4 text-primary" />
                          </Button>
                        )}
                        {isAdmin && (
                          <>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleEdit(book)}
                            >
                              <Edit className="h-4 w-4" />
                            </Button>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleDelete(book.id)}
                            >
                              <Trash2 className="h-4 w-4 text-destructive" />
                            </Button>
                          </>
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

