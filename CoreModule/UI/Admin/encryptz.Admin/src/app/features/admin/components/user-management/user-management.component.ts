import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { AddEditUserComponent } from './add-edit-user/add-edit-user.component';
import { UserManagementService } from './user-management.service';
import { MatCardModule } from "@angular/material/card";
import { MatPaginatorModule } from '@angular/material/paginator';
import { CommonService } from '../../../../shared/services/common.service';
import { ConfirmationComponent } from '../../../../shared/components/confirmation/confirmation.component';

@Component({
  selector: 'app-user-management',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatPaginatorModule
  ],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.scss'
})
export class UserManagementComponent {

  userList: any

  constructor(
    private dialog: MatDialog,
    private service: UserManagementService,
    private commonService: CommonService
  ) { }

  ngOnInit() {
    this.getAllUsers()
  }

  addEditUser(type: number = 1, info: any = []) {
    const dialogRef = this.dialog.open(AddEditUserComponent, {
      width: '900px',
      data: {
        type: type,
        title: type == 1 ? 'Create New User' : 'Update ' + info.userName,
        info: info
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.getAllUsers()
      }
    })
  }


  getAllUsers() {
    this.service.getAllUser().subscribe({
      next: (res: any) => {
        this.userList = res
      },
      error: (err: any) => {
        this.commonService.showSnackbar(err.message || 'Error occurred while loading user information', 'ERROR', 3000);
      }
    })
  }

  deleteUser(userInfo: any) {
    const dialogRef = this.dialog.open(ConfirmationComponent, {
      width: '400px',
      data: {
        title: 'Confirmation for delete',
        description: `Are you sure you want to delete " ${userInfo.userName} " from the master?`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        let payload = {
          id : userInfo.id
        }
        this.service.deleteUser(userInfo.id, payload).subscribe({
          next: (res: any) => {
          debugger
          if (res) {
            this.commonService.showSnackbar(res.message, 'SUCCESS', 3000);
            this.getAllUsers()
          }
        },
        error: (err: any) => {
          this.commonService.showSnackbar(err.message, 'ERROR', 3000)
        }
        })
      }
    });
  }

}
