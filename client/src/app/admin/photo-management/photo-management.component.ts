import { Component, inject, OnInit, signal } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css'
})
export class PhotoManagementComponent implements OnInit {
  private adminService = inject(AdminService);
  photos = signal<Photo[]>([]);
  
  ngOnInit() {
    this.loadPhotos();
  }

  loadPhotos() {
    this.adminService.getPhotosForModeration().subscribe({
      next: response => this.photos.set(response)
    });
  }

  approvePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).subscribe({
      next: () => this.photos.update(photos => photos.filter(x => x.id != photoId))
    })
  }

  rejectPhoto(photoId: number) {
    this.adminService.rejectPhoto(photoId).subscribe({
      next: () => this.photos.update(photos => photos.filter(x => x.id != photoId))
    })
  }
}
