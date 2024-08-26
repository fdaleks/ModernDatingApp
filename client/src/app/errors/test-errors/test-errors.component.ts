import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-test-errors',
  standalone: true,
  imports: [],
  templateUrl: './test-errors.component.html',
  styleUrl: './test-errors.component.css'
})
export class TestErrorsComponent {
  baseUrl: string = environment.apiUrl;
  private httpClient = inject(HttpClient);
  validationErrors: string[] = [];

  get400error() {
    this.httpClient.get(this.baseUrl + 'buggy/bad-request').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get401error() {
    this.httpClient.get(this.baseUrl + 'buggy/auth').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get404error() {
    this.httpClient.get(this.baseUrl + 'buggy/not-found').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get500error() {
    this.httpClient.get(this.baseUrl + 'buggy/server-error').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get400validationError() {
    this.httpClient.post(this.baseUrl + 'account/register', {}).subscribe({
      next: response => console.log(response),
      error: error => { 
        console.log(error)
        this.validationErrors = error;
      }
    });
  }
}
