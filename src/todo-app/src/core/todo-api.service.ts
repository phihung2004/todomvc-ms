import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {environment} from '../environments/environment';

// Khai báo khuôn dữ liệu (y hệt thằng DTO ở BE)
export interface TodoDto {
  id: string;
  title: string;
  isCompleted: boolean;
  createAt: string;
}

export interface CreateTodoRequest {
  title: string;
}

export interface UpdateTodoRequest {
  title: string;
  isCompleted: boolean
}

@Injectable({
  providedIn: 'root',
})
export class TodoApiService {
  // GetById, Create, Update, Toggle, Delete, DeleteCompleted
  
  private readonly apiUrl = environment.apiUrl;
  //private readonly apiUrl = 'http://localhost:5100/bff/todos';

  // Inject HttpClient để gọi API
  private http = inject(HttpClient);

  // Hàm gọi GET lấy toàn bộ data
  getTodos(): Observable<TodoDto[]> {
    return this.http.get<TodoDto[]>(this.apiUrl);
  }

  // Hàm gọi GET lấy toàn bộ data
  getById(id:string): Observable<TodoDto> {
    return this.http.get<TodoDto>(`${this.apiUrl}/${id}`);
  }

  createTodo(request:CreateTodoRequest): Observable<TodoDto>{
    return this.http.post<TodoDto>(this.apiUrl,request);
  }

  updateTodo(id:string,request:UpdateTodoRequest): Observable<TodoDto>{
    return this.http.put<TodoDto>(`${this.apiUrl}/${id}`,request);
  }

  toggleTodo(id:string): Observable<TodoDto>{
    return this.http.patch<TodoDto>(`${this.apiUrl}/${id}/toggle`,null);
  }

  deleteTodo(id:string): Observable<TodoDto>{
    return this.http.delete<TodoDto>(`${this.apiUrl}/${id}`);
  }

  deleteCompleted(): Observable<TodoDto>{
    return this.http.delete<TodoDto>(`${this.apiUrl}/completed`);
  }
}
