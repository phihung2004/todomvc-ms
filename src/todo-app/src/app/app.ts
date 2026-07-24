import { Component } from '@angular/core';
import { TodoInput } from '../features/todos/todo-input/todo-input';
import { Footer } from '../features/todos/footer/footer';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  imports: [TodoInput,Footer, RouterOutlet],
})
export class App {
  //activeRouter: Inject(ActivatedRoute);
}
