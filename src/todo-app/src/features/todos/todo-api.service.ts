import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../core/environments/environment';
// Khai báo khuôn dữ liệu (y hệt thằng DTO ở BE)
export interface TodoDto {
  id: string;
  title: string;
  isCompleted: boolean;
  createAt: string;
  dueAt?: string | Date;
}
export interface CreateTodoRequest {
  title: string;
  dueAt?: string | Date;
}

export interface UpdateTodoRequest {
  title: string;
  isCompleted: boolean;
  dueAt?: string | Date;
}

@Injectable({
  providedIn: 'root',
})
export class TodoApiService {
  // GetById, Create, Update, Toggle, Delete, DeleteCompleted

  private readonly apiUrl = environment.apiUrl + '/todos';
  //private readonly apiUrl = 'http://localhost:5100/bff/todos';

  // Inject HttpClient để gọi API
  private http = inject(HttpClient);

  // Hàm gọi GET lấy toàn bộ data
  getTodos(filter?: string): Observable<TodoDto[]> {
    // Chưa ổn vì nếu không có filter nào thì nó sẽ gửi về là undefined tahy vì null
    //return this.http.get<TodoDto[]>(`${this.apiUrl}?filter=${filter}`);

    // Dùng params
    let params = new HttpParams();
    if (filter && filter != 'all') {
      params = params.set('filter', filter);
    }

    return this.http.get<TodoDto[]>(this.apiUrl, { params });
  }

  // Hàm gọi GET lấy toàn bộ data
  getById(id: string): Observable<TodoDto> {
    return this.http.get<TodoDto>(`${this.apiUrl}/${id}`);
  }

  createTodo(request: CreateTodoRequest): Observable<TodoDto> {
    return this.http.post<TodoDto>(this.apiUrl, request);
  }

  updateTodo(id: string, request: UpdateTodoRequest): Observable<TodoDto> {
    return this.http.put<TodoDto>(`${this.apiUrl}/${id}`, request);
  }

  toggleTodo(id: string): Observable<TodoDto> {
    return this.http.patch<TodoDto>(`${this.apiUrl}/${id}/toggle`, null);
  }

  deleteTodo(id: string): Observable<TodoDto> {
    return this.http.delete<TodoDto>(`${this.apiUrl}/${id}`);
  }

  deleteCompleted(): Observable<TodoDto> {
    return this.http.delete<TodoDto>(`${this.apiUrl}/completed`);
  }
}
