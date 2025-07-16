import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { AddEditUserComponent } from './add-edit-user/add-edit-user.component';
import { UserManagementService } from './user-management.service';
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
  constructor(
    private dialog: MatDialog,
    private service: UserManagementService
  ) {

  }

  ngOnInit() {
    this.getAllUsers()
  }

  addEditUser(type: number = 1, info: any = []) {
    const dialogRef = this.dialog.open(AddEditUserComponent, {
      width: '900px',
      data: {
        type: type,
        title: 'Create New User',
        info: info
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.getAllUsers()
      }
    })
  }


  getAllUsers(){
    this.service
  }

}
