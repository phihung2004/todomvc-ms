import { Component, inject } from '@angular/core';
import { AsyncPipe} from '@angular/common';
import { TodosStore } from '../todos.store';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-footer',
  imports: [AsyncPipe, RouterLink, RouterLinkActive],
  templateUrl: './footer.html',
})
export class Footer {
    store = inject(TodosStore)

    clearCompleted(): void{
      this.store.clearCompleted();
    }

    setFilter(filter: 'all' | 'active' | 'completed'): void{
      this.store.setFilter(filter);
    }
}
