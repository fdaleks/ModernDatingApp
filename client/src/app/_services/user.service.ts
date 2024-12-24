import { computed, Injectable, signal } from '@angular/core';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  currentUser = signal<User | null>(null);
  roles = computed(() => {
    const user = this.currentUser();
    if (user && user.token) {
      const payload = user.token.split('.')[1];
      const role = JSON.parse(atob(payload)).role;
      return Array.isArray(role) ? role : [role];
    }
    return [];
  });
  
  setCurrentUser(user: User, setUserToStorage: boolean) {
    if (setUserToStorage) {
      localStorage.setItem('user', JSON.stringify(user));
    }
    this.currentUser.set(user);
  }

  setUserMainPhoto(photoUrl: string) {
    const user = this.currentUser();
    if (user) {
      user.photoUrl = photoUrl;
      this.setCurrentUser(user, true);
    }
  }

  unsetCurrentUser() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }
}
