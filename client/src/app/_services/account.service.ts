import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { User } from '../_models/user';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private httpClient = inject(HttpClient);
  private likesService = inject(LikesService);
  private baseUrl: string = environment.apiUrl;
  currentUser = signal<User | null>(null);

  register(registerModel: any) {
    return this.httpClient.post<User>(this.baseUrl + 'account/register', registerModel).pipe(
      map(newUser => {
        if (newUser) this.setCurrentUser(newUser);
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
    this.likesService.getCurrentUserLikeIds();
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }

}
