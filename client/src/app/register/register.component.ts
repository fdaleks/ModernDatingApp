import { Component, inject, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private accountService = inject(AccountService);
  cancelRegister = output<boolean>();
  registerModel: any = {};

  register() {
    this.accountService.register(this.registerModel).subscribe({
      next: response => {
        console.log(response);
        this.cancel();
      },
      error: error => console.log(error),
      complete: () => console.log('Request has completed!')
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}