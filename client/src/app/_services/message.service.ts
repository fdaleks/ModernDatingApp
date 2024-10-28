import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult } from '../_models/pagination';
import { Message } from '../_models/message';
import { setPaginatedResponse, setPaginationHeaders } from '../_helpers/paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private httpClient = inject(HttpClient);
  private baseUrl: string = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);

  sendMessage(userName: string, content: string) {
    return this.httpClient.post<Message>(this.baseUrl + 'messages', { recipientUserName: userName, content: content });
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = setPaginationHeaders(pageNumber, pageSize);
    params = params.append('container', container);

    return this.httpClient.get<Message[]>(this.baseUrl + 'messages', { observe: 'response', params }).subscribe({
      next: response => setPaginatedResponse(this.paginatedResult, response)
    });
  }

  getMessagesThread(userName: string) {
    return this.httpClient.get<Message[]>(this.baseUrl + 'messages/thread/' + userName);
  }

  deleteMessage(messageId: number) {
    return this.httpClient.delete(this.baseUrl + 'messages/' + messageId);
  }
}
