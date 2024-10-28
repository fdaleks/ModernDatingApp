import { Component, inject, input, output, ViewChild } from '@angular/core';
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';
import { TimeagoModule } from 'ngx-timeago';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [TimeagoModule, FormsModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css'
})
export class MemberMessagesComponent {
  @ViewChild('messageForm') messageForm?: NgForm;
  private messageService = inject(MessageService);
  userName = input.required<string>();
  messages = input.required<Message[]>();
  messageContent: string = '';
  updateMessages = output<Message>();

  sendMessage() {
    this.messageService.sendMessage(this.userName(), this.messageContent).subscribe({
      next: response => {
        this.updateMessages.emit(response);
        this.messageForm?.reset();
      }
    });
  }

}
