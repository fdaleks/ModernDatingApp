import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { User } from '../_models/user';
import { take } from 'rxjs';
import { Router } from '@angular/router';
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  private hubsUrl: string = environment.hubsUrl;
  private hubConnection?: HubConnection;
  private userService = inject(UserService);
  private toastr = inject(ToastrService);
  private router = inject(Router);
  onlineUsers = signal<string[]>([]);

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubsUrl + 'presence', {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(error => console.log(error));

    this.hubConnection.on('UserIsOnline', userName => {
      this.onlineUsers.update(users => [...users, userName])
    });

    this.hubConnection.on('UserIsOffline', userName => {
      this.onlineUsers.update(users => users.filter(user => user != userName))
    });

    this.hubConnection.on('GetOnlineUsers', userNames => {
      this.onlineUsers.set(userNames);
    });

    this.hubConnection.on('NewMessageReceived', ({userName, knownAs}) => {
      this.toastr.info(knownAs + ' has sent you a new message! Click me to see it!')
        .onTap
        .pipe(take(1))
        .subscribe(() => this.router.navigateByUrl('/members/' + userName + '?tab=Messages'));
    });

    this.hubConnection.on('UpdateMainPhoto', photoUrl => {
      this.userService.setUserMainPhoto(photoUrl);
    });
  }

  stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }
}
