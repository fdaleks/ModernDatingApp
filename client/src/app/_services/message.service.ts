import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { Message } from '../_models/message';
import { setPaginatedResponse, setPaginationHeaders } from '../_helpers/paginationHelper';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { User } from '../_models/user';
import { Group } from '../_models/group';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private httpClient = inject(HttpClient);
  private baseUrl: string = environment.apiUrl;
  private hubsUrl: string = environment.hubsUrl;
  hubConnection?: HubConnection;
  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);
  messageThread = signal<Message[]>([]);
  
  createHubConnection(user: User, recipientUserName: string) {
    this.hubConnection = new HubConnectionBuilder()
    .withUrl(this.hubsUrl + 'messages?user=' + recipientUserName, {
      accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build();

    this.hubConnection.start().catch(error => console.log(error));

    this.hubConnection.on('ReceiveMessageThread', messages => {
      this.messageThread.set(messages);
    });

    this.hubConnection.on('NewMessage', message => {
      this.messageThread.update(messages => [...messages, message]);
    });

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      if (group.connections.some(x => x.userName === recipientUserName)) {
        this.messageThread.update(messages => {
          messages.forEach(message => {
            if (!message.dateRead) {
              message.dateRead = new Date(Date.now());
            }
          });
          return messages;
        })
      }
    });
  }

  stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }

  async sendMessage(userName: string, content: string) {
    return this.hubConnection?.invoke('SendMessage', { recipientUserName: userName, content: content });
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = setPaginationHeaders(pageNumber, pageSize);
    params = params.append('container', container);

    return this.httpClient.get<Message[]>(this.baseUrl + 'messages', { observe: 'response', params }).subscribe({
      next: response => setPaginatedResponse(this.paginatedResult, response)
    });
  }

  deleteMessage(messageId: number) {
    return this.httpClient.delete(this.baseUrl + 'messages/' + messageId);
  }
}
