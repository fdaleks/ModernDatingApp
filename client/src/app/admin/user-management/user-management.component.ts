import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { User } from '../../_models/user';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from '../../modals/roles-modal/roles-modal.component';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  bsModalRef: BsModalRef<RolesModalComponent> = new BsModalRef<RolesModalComponent>();
  private adminService = inject(AdminService);
  private modalService = inject(BsModalService);
  users: User[] = [];

  ngOnInit(): void {
    this.loadUsers();
  }

  openRolesModal(user: User) {
    const initialState: ModalOptions = {
      class: 'modal-lg',
      initialState: {
        title: 'User roles',
        userName: user.userName,
        selectedRoles: [...user.roles],
        availableRoles: ['Admin', 'Moderator', 'Member'],
        rolesUpdated: false
      }
    }
    this.bsModalRef = this.modalService.show(RolesModalComponent, initialState);
    this.bsModalRef.onHide?.subscribe({
      next: () => {
        if (this.bsModalRef.content && this.bsModalRef.content.rolesUpdated) {
          const selectedRoles = this.bsModalRef.content.selectedRoles;
          this.adminService.updateUserRoles(user.userName, selectedRoles).subscribe({
            next: response => user.roles = response
          });
        }
      }
    });
  }

  loadUsers() {
    this.adminService.getUsersWithRoles().subscribe({
      next: response => this.users = response
    });
  }

}