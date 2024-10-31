import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private httpClient = inject(HttpClient);
  private baseUrl: string = environment.apiUrl;

  getUsersWithRoles() {
    return this.httpClient.get<User[]>(this.baseUrl + 'admin/users-with-roles');
  }

  updateUserRoles(userName: string, roles: string[]) {
    return this.httpClient.post<string[]>(this.baseUrl + 'admin/edit-roles/' + userName + '?roles=' + roles, {})
  }
}
