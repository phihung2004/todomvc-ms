import { inject, Injectable, signal } from '@angular/core';
import { TodoApiService, TodoDto } from './todo-api.service';
import { ComponentStore } from '@ngrx/component-store';
import { tapResponse } from '@ngrx/operators';
import { switchMap, concatMap, withLatestFrom, tap, EMPTY } from 'rxjs';

export type FilterType = 'all' | 'active' | 'completed';

// State interface
// Kho tổng
export interface TodosState {
  todos: TodoDto[]; // Nguyên mảng của todo
  filter: FilterType; // Lưu trạng tahis của filter
  loading: boolean; // Lưu biến trạng thái tổng
  error: string | null; // lưu thông báo lỗi
}

@Injectable({
  providedIn: 'root',
})
export class TodosStore extends ComponentStore<TodosState> {
  private readonly todoApiService = inject(TodoApiService);

  constructor() {
    super({
      todos: [],
      filter: 'all',
      loading: false,
      error: null,
    });
  }

  // Selectors============================================
  // Lấy
  readonly todos$ = this.select((state) => state.todos);

  readonly filter$ = this.select((state) => state.filter);

  readonly filteredTodos$ = this.select(this.todos$, this.filter$, (todos, filter) => {
    if (filter === 'active') return todos.filter((t) => !t.isCompleted);
    if (filter === 'completed') return todos.filter((t) => t.isCompleted);
    return todos; // Nếu filter === 'all'
  });

  readonly loading$ = this.select((state) => state.loading);

  readonly error$ = this.select((state) => state.error);

  // Tự đếm số lượng Active từ mảng gốc
  readonly activecount$ = this.select(
    this.todos$,
    (todos) => todos.filter((todo) => !todo.isCompleted).length,
  );

  readonly isAllCompleted$ = this.select(
    this.todos$,
    (todos) => todos.length > 0 && todos.every((todo) => todo.isCompleted),
  );

  // Tự đếm xem có cái nào Completed không
  readonly hasCompleted$ = this.select(this.todos$, (todos) =>
    todos.some((todo) => todo.isCompleted),
  );

  readonly hasTodo$ = this.select(this.todos$, (todos) => todos.length > 0);

  // Updater============================================
  // Thằng duy nhất sẽ chạm vào cục State - Kho todos trên cùng để sửa.
  // Sẽ là các điều kiện để GIỮ LẠI những cái todos cũ, hoặc thêm mới, hoặc xóa đi, hoặc update lại.

  // Đổi kiểu đầu vào của todos thành TodoListResponse
  readonly setTodos = this.updater(
    (state, response: import('./todo-api.service').TodoListResponse) => ({
      ...state,
      todos: response.items, // Rút đúng cái mảng ra khỏi hộp
    }),
  );

  // cách đọc thằng updater : Tên_Hàm + <Bơ_Đi> + (Đầu_Vào) + : + Đầu_Ra
  readonly appendTodo = this.updater((state, todo: TodoDto) => ({
    ...state,
    todos: [...state.todos, todo],
  }));

  readonly updateTodoInStore = this.updater((state, updatedTodo: TodoDto) => ({
    ...state,
    todos: state.todos.map((todo) => (todo.id === updatedTodo.id ? updatedTodo : todo)), // Tìm, đúng id thì nó update giá trị, right ?
  }));

  // Chỉ tìm đúng ID và đổi Title, giữ nguyên các trường khác (như createAt)
  // M7: agiowf nó sửa thêm cả Date, nên giwof sẽ đổi tên
  // Từ editTitleInStore => editTodoInStore
  // Đổi tên và thêm dueAt vào payload
  readonly editTodoInStore = this.updater(
    (state, payload: { id: string; title: string; dueAt?: string }) => ({
      ...state,
      todos: state.todos.map((todo) =>
        todo.id === payload.id
          ? { ...todo, title: payload.title, dueAt: payload.dueAt } // Cập nhật cả Date
          : todo,
      ),
    }),
  );

  readonly toggleInStore = this.updater((state, isCompletedTarget: boolean) => ({
    ...state,
    todos: state.todos.map((todo) => ({ ...todo, isCompleted: isCompletedTarget })),
  }));

  readonly removeTodoFromStore = this.updater((state, todoId: string) => ({
    ...state,
    todos: state.todos.filter((todo) => todo.id !== todoId),
  }));

  readonly removeCompletedTodosFromStore = this.updater((state) => ({
    ...state,
    todos: state.todos.filter((todo) => !todo.isCompleted), // Lọc
  }));

  readonly setFilter = this.updater((state, filter: FilterType) => ({
    ...state,
    filter,
  }));

  readonly setLoading = this.updater((state, loading: boolean) => ({
    ...state,
    loading,
  }));

  readonly setError = this.updater((state, error: string | null) => ({
    ...state,
    error,
  }));

  // Tìm đúng cái ID đó và tự động lật ngược isCompleted (true -> false, false -> true)
  readonly toggleSingleTodoInStore = this.updater((state, id: string) => ({
    ...state,
    todos: state.todos.map((todo) =>
      todo.id === id ? { ...todo, isCompleted: !todo.isCompleted } : todo,
    ),
  }));

  //Effects============================================
  readonly loadTodos = this.effect<void>((trigger$) =>
    trigger$.pipe(
      tap(() => this.setLoading(true)),
      switchMap(() =>
        this.todoApiService.getTodos('all').pipe(
          tapResponse(
            (response) => {
              this.setTodos(response); // response truyền vào đây là TodoListResponse
              this.setLoading(false);
            },
            (error) => {
              console.error('Error loading todos:', error);
              this.setLoading(false);
              this.setError("Can't load TodoList");
            },
          ),
        ),
      ),
    ),
  );

  readonly createTodo = this.effect<{ title: string; dueAt?: string }>((payload$) =>
    payload$.pipe(
      // concatMap: Tạo hàng đợi (Queue). Bấm liên tục 5 phát thì xử lý tuần tự từng cái một, không bỏ sót cái nào
      concatMap((payload) => {
        const trimmedTitle = payload.title.trim();

        // Đang test Validation dưới BE nên comment, mà, cho BE luôn đi, bỏ công làm thì cho Craete xuống BE đi
        // if (!trimmedTitle) {
        //   this.setError('Todo can not leave emty!');
        //   return EMPTY; // Báo lỗi xong thì Dừng luồng (EMPTY), không gọi API nữa.
        // }

        // if (trimmedTitle.length > 200) {
        //   this.setError('Todo title no more than 200 characters!');
        //   return EMPTY;
        // }

        this.setError(null);

        return this.todoApiService.createTodo({ title: trimmedTitle, dueAt: payload.dueAt }).pipe(
          tapResponse(
            (newTodo) => this.appendTodo(newTodo), // Thành công: Nhét vào mảng
            // (error) => this.setError('Error creating todo'),
            (error: any) => {
              console.log('Lỗi từ BE:', error); // Log ra console để xem

              // Bóc tách JSON ProblemDetails của BE
              // Thường Angular HttpClient sẽ nhét cái body lỗi vào properties `error`
              // Khớp với code BE của ông: problemDetails.Extensions["errors"]
              const beErrors = error.error?.errors;

              if (beErrors && beErrors.length > 0) {
                // Lấy cái Message của lỗi đầu tiên (Sẽ là câu "...skibidi" của ông)
                // Lưu ý: JSON deserialize có thể biến chữ hoa thành chữ thường (Message -> message)
                const errorMessage = beErrors[0].Message || beErrors[0].message;
                this.setError(errorMessage);
              } else {
                // Nếu BE lỗi gì khác (sập server, rớt mạng...)
                this.setError('Error creating todo from Server');
              }
            },
          ),
        );
      }),
    ),
  );

  // Đã được AI sửa để fix được lỗi là cần F5 để hiện update
  readonly updateTodo = this.effect<{
    id: string;
    title: string;
    isCompleted: boolean;
    dueAt?: string;
  }>((payload$) =>
    payload$.pipe(
      tap((payload) =>
        this.editTodoInStore({ id: payload.id, title: payload.title, dueAt: payload.dueAt }),
      ),

      concatMap((payload) =>
        this.todoApiService
          .updateTodo(payload.id, {
            title: payload.title,
            isCompleted: payload.isCompleted,
            dueAt: payload.dueAt,
          })
          .pipe(
            tapResponse(
              () => console.log('Đã lưu Title mới xuống Backend!'),
              (error) => {
                console.error('Error updating todo:', error);
                this.loadTodos();
              },
            ),
          ),
      ),
    ),
  );

  // readonly toggleTodo = this.effect<string>((id$) =>
  //   id$.pipe(
  //     concatMap((id) =>
  //       this.todoApiService.toggleTodo(id).pipe(
  //         tapResponse(
  //           (updatedItem) => this.updateTodoInStore(updatedItem), // API trả về item mới -> Cập nhật UI
  //           (error) => console.error('Error toggling todo:', error)
  //         )
  //       )
  //     )
  //   )
  // );

  // Khai báo một Effect, nhận đầu vào là chuỗi string (chính là cái ống id$)
  readonly toggleTodo = this.effect<string>((id$) =>
    // Bắt đầu đưa cái ống id$ vào băng chuyền xử lý
    id$.pipe(
      // Trạm 1: tap - Đứng nhìn và làm việc vặt
      // tap() giống như một thằng bảo vệ đứng nhìn cái ID đi qua. Nó lấy cái ID đó,
      // gọi ngay anh Công nhân (toggleSingleTodoInStore) để đổi màu UI lập tức.
      // Xong việc, nó thả cái ID đi tiếp xuống Trạm 2.
      tap((id) => this.toggleSingleTodoInStore(id)),

      // Trạm 2: concatMap - Xếp hàng gọi điện (API)
      // Khi cái ID rơi xuống đây, nó bắt đầu gọi điện lên Server Backend.
      // concatMap có tính năng "Xếp hàng": Nếu ông bấm 3 nút liên tục, nó sẽ đợi API thứ 1 gọi xong
      // mới gọi tiếp API thứ 2, không bao giờ bị đè lệnh.
      concatMap((id) =>
        this.todoApiService.toggleTodo(id).pipe(
          // Gọi HTTP Request

          // Trạm 3: tapResponse - Đón kết quả từ Server trả về
          tapResponse(
            // Nếu API báo 204 Thành công: Hê hê, tao lừa user update UI từ bước 1 rồi, nên giờ chả cần làm gì cả.
            () => {
              console.log('Đã đồng bộ trạng thái Toggle với Server');
            },

            // Nếu API báo LỖI (Vd: sập mạng): Chết dở!
            (error) => {
              console.error('Error toggling todo:', error);
              // Phải gọi anh Công nhân ra bốc ngược lại kho (Lật UI lại như cũ) để user biết mạng đang lag.
              this.toggleSingleTodoInStore(id);
            },
          ),
        ),
      ),
    ),
  );

  // Dùng AI cho bên dưới :)))
  // Xử lý Toggle All xuống Backend
  // Coi doc của thằng withLatestFrom
  // Trở lại nhận biến boolean từ UI truyền xuống
  // isCompletedTarget nghĩa là : Trang thái mục tiêu của nút check, dựa vào thằng Toggle All
  // Khai báo Effect, nhận đầu vào là boolean (true = muốn check hết, false = muốn gỡ hết)
  readonly toggleAllTodos = this.effect<boolean>((isCompletedTarget$) =>
    // Đưa biến boolean vào băng chuyền
    isCompletedTarget$.pipe(
      // Trạm 1: withLatestFrom - Chụp lén cái Kho
      // Chữ "Target" chỉ là true hoặc false, ta không biết trong list có cái gì.
      // Nên thằng này có nhiệm vụ: "Ê, lôi mảng 'todos' hiện tại trong kho ra đây cho tao đối chiếu!"
      withLatestFrom(this.todos$),

      // Trạm 2: tap - Bắt đầu thi công
      // Lúc này tap() nhận được 1 mảng gồm 2 món: [Cái đích muốn đến, Cái mảng hiện tại]
      tap(([isCompletedTarget, todos]) => {
        // 1. Dùng hàm filter của Javascript: Soi xem thằng Todo nào ĐANG KHÁC với cái đích thì giữ lại.
        // (Để tránh việc nó đang tick xanh rồi mình lại gửi API bắt nó tick xanh phát nữa)
        const itemsToUpdate = todos.filter((t) => t.isCompleted !== isCompletedTarget);

        // 2. Chạy vòng lặp qua những thằng cần sửa
        itemsToUpdate.forEach((item) => {
          // Khúc này Ảo Diệu này: Thay vì tự viết gọi API, nó gọi cmn lại cái Effect toggleTodo ở phía trên luôn!
          // Mà Effect ở trên thì đã có sẵn tính năng "Tự lật UI + Xếp hàng gọi API an toàn". Cực nhàn!
          this.toggleTodo(item.id);
        });

        // 3. Gọi anh Công nhân tổng (toggleInStore)
        // Dù cái vòng lặp ở trên đã tự lật UI từng thằng, nhưng gọi phát này để chốt hạ
        // lật xanh lè cả mảng cùng 1 tích tắc, đảm bảo không có độ trễ thị giác nào.
        this.toggleInStore(isCompletedTarget);
      }),
    ),
  );

  // cầm string id đưa về cho service gọi API
  readonly deleteTodo = this.effect<string>((id$) =>
    id$.pipe(
      concatMap((id) =>
        this.todoApiService.deleteTodo(id).pipe(
          tapResponse(
            () => this.removeTodoFromStore(id), // Thành công thì rút nó ra khỏi danh sách UI
            (error) => console.error('Error deleting todo:', error),
          ),
        ),
      ),
    ),
  );

  // cứ gọi là chạy, không có tham số
  readonly clearCompleted = this.effect<void>((trigger$) =>
    trigger$.pipe(
      concatMap(() =>
        this.todoApiService.deleteCompleted().pipe(
          tapResponse(
            () => this.removeCompletedTodosFromStore(), // Quét sạch UI
            (error) => console.error('Error clearing completed todos:', error),
          ),
        ),
      ),
    ),
  );

  // Mãu thử dùng basic mà không chơi Component Store gì đó
  // readonly todos = signal<TodoDto[]>([]);

  // private readonly todoApiService = inject(TodoApiService);

  // loadTodos(): void {
  //   this.todoApiService.getTodos().subscribe({
  //     next: (data) => {
  //       this.todos.set(data);
  //     },
  //     error: (err) => {
  //       console.error('Error loading todos:', err);
  //     }
  //   });
  // }

  // createTodo(title: string): void {
  //   const request = { title: title };

  //   this.todoApiService.createTodo(request).subscribe({
  //     next: (newTodo) => {
  //       this.todos.update((currentTodos) => [...currentTodos, newTodo]);
  //     },
  //     error: (err) => {
  //       console.error('Error creating todo:', err);
  //     }
  //   });
  // }
}
