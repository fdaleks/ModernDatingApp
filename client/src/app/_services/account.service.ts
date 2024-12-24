import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { User } from '../_models/user';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';
import { LikesService } from './likes.service';
import { PresenceService } from './presence.service';
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private httpClient = inject(HttpClient);
  private userService = inject(UserService);
  private likesService = inject(LikesService);
  private presenceService = inject(PresenceService);
  private baseUrl: string = environment.apiUrl;

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

  setCurrentUser(user: User, setToStorage: boolean = true) {
    this.userService.setCurrentUser(user, setToStorage);
    this.likesService.getCurrentUserLikeIds();
    this.presenceService.createHubConnection(user);
  }

  logout() {
    this.userService.unsetCurrentUser();
    this.presenceService.stopHubConnection();
  }

}
