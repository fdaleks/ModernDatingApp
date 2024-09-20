import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../_services/account.service';
import { environment } from '../../../environments/environment';
import { MembersService } from '../../_services/members.service';
import { Photo } from '../../_models/photo';

@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [NgIf, NgFor, NgStyle, NgClass, FileUploadModule, DecimalPipe],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css'
})
export class PhotoEditorComponent implements OnInit {
  member = input.required<Member>();
  memberChange = output<Member>();
  hasBaseDropZoneOver: boolean = false;
  private accountService = inject(AccountService);
  private membersService = inject(MembersService);
  private baseUrl: string = environment.apiUrl;
  uploader?: FileUploader;

  ngOnInit(): void {
    this.initializeUploader();
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer ' + this.accountService.currentUser()?.token,
      maxFileSize: 1 * 1024 * 1024,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      isHTML5: true
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      const photo = JSON.parse(response);
      const updatedMember = {...this.member()};
      updatedMember.photos.push(photo);
      if (photo.isMain) {
        const user = this.accountService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accountService.setCurrentUser(user);
        }
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.map(x => x.isMain = x.id === photo.id ? true : false);
      }
      this.memberChange.emit(updatedMember);
    }
  }

  fileOverBase(event: any) {
    this.hasBaseDropZoneOver = event;
  }

  setMainPhoto(photo: Photo) {
    this.membersService.setMainPhoto(photo).subscribe({
      next: () => {
        const user = this.accountService.currentUser();
        if (user) {
          user.photoUrl = photo.url;
          this.accountService.setCurrentUser(user);
        }
        const updatedMember = {...this.member()};
        updatedMember.photoUrl = photo.url;
        updatedMember.photos.map(x => x.isMain = x.id === photo.id ? true : false);
        this.memberChange.emit(updatedMember);
      }
    });
  }

  deletePhoto(photo: Photo) {
    this.membersService.deletePhoto(photo).subscribe({
      next: () => {
        const updatedMember = {...this.member()};
        updatedMember.photos = updatedMember.photos.filter(x => x.id !== photo.id);
        this.memberChange.emit(updatedMember);
      }
    });
  }

}
