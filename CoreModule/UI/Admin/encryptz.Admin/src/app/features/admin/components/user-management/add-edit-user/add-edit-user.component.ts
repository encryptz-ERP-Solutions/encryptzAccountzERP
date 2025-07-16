import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { UserManagementService } from '../user-management.service';

@Component({
  selector: 'app-add-edit-user',
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatInputModule,
    MatSelectModule
  ],
  templateUrl: './add-edit-user.component.html',
  styleUrl: './add-edit-user.component.scss'
})
export class AddEditUserComponent {
  userInfoForm !: FormGroup
  constructor(
    private dialogRef: MatDialogRef<AddEditUserComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private service: UserManagementService
  ) {
    this.dialogRef.disableClose = true
  }


  ngOnInit() {
    this.userInfoForm = new FormGroup({
      userName: new FormControl('', Validators.required),
      password: new FormControl('', Validators.required),
      PANnumber: new FormControl('', Validators.required),
      aadharNumber: new FormControl('', Validators.required),
      phoneNumber: new FormControl('', Validators.required),
      email: new FormControl('', Validators.required),
      state: new FormControl('', Validators.required),
      nation: new FormControl('', Validators.required)
    })
  }



  saveUser() {
    // if (this.userInfoForm.valid) {
    let payload = {
      "userId": 0,
      "userName": "Said Mohammed",
      "userPassword": "123",
      "email": "seyd@gmal.com",
      "panNo": "12345HGT",
      "adharCardNo": "123456787896",
      "phoneNo": "9878976545",
      "address": "Pisharath House, Malappuram, Moorkkanad",
      "stateId": 12,
      "nationId": 23,
      "isActive": true
    }
    this.service.createNewUser(payload).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.dialogRef.close(true)
        }
      },
      error: (res: any) => {
        debugger
      }
    })
  }
}
