import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-user-management',
  imports: [
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss'
})
export class UserManagementComponent {

}
