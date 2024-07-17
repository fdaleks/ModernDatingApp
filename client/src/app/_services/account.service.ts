import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { UserModel } from '../_models/user.model';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private httpClient = inject(HttpClient);
  baseUrl: string = 'https://localhost:5001/api/';
  currentUser = signal<UserModel | null>(null);

  register(registerModel: any) {
    return this.httpClient.post<UserModel>(this.baseUrl + 'account/register', registerModel).pipe(
      map(newUser => {
        if (newUser) {
          localStorage.setItem('user', JSON.stringify(newUser));
          this.currentUser.set(newUser);
        }
        return newUser;
      })
    );
  }
  
  login(loginModel: any) {
    return this.httpClient.post<UserModel>(this.baseUrl + 'account/login', loginModel).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUser.set(user);
        }
        return user;
      })
    );
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }

}
