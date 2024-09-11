import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { User } from '../_models/user';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private httpClient = inject(HttpClient);
  private baseUrl: string = environment.apiUrl;
  currentUser = signal<User | null>(null);

  register(registerModel: any) {
    return this.httpClient.post<User>(this.baseUrl + 'account/register', registerModel).pipe(
      map(newUser => {
        if (newUser) this.setCurrentUser(newUser);
        //return newUser;
      })
    );
  }

  login(loginModel: any) {
    return this.httpClient.post<User>(this.baseUrl + 'account/login', loginModel).pipe(
      map(user => {
        if (user) this.setCurrentUser(user);
      })
    );
  }

  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }

}
