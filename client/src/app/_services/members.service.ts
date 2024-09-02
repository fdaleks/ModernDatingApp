import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private httpClient = inject(HttpClient);
  private baseUrl: string = environment.apiUrl;
  members = signal<Member[]>([]);

  getMembers() {
    return this.httpClient.get<Member[]>(this.baseUrl + 'users').subscribe({
      next: members => this.members.set(members)
    });
  }

  getMember(userName: string) {
    const member = this.members().find(x => x.userName === userName);
    
    if (member !== undefined) return of(member);
    return this.httpClient.get<Member>(this.baseUrl + 'users/' + userName);
  }

  updateMember(member: Member) {
    return this.httpClient.put(this.baseUrl + 'users', member).pipe(
      tap(() => {
        this.members.update(members => members.map(x => x.userName === member.userName ? member : x));
      })
    );
  }
}
