import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [FormsModule, BsDropdownModule],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {
  public accountService = inject(AccountService);
  loginModel: any = {};

  login() {
    this.accountService.login(this.loginModel).subscribe({
      next: response => console.log(response),
      error: error => console.log(error),
      complete: () => console.log('Request has completed!')
    });
  }

  logout() {
    this.accountService.logout();
  }
}
