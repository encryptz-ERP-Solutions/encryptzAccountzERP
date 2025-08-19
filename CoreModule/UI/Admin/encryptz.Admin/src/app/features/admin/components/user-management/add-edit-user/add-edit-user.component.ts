import { Component, Inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { UserManagementService } from '../user-management.service';
import { CommonService } from '../../../../../shared/services/common.service';

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
    private service: UserManagementService,
    private commonService: CommonService,
  ) {
    this.dialogRef.disableClose = true
  }


  ngOnInit() {
    this.initForm()
    if (this.data.type == 2) {
      debugger
      this.userInfoForm.patchValue(this.data.info)
    }
  }

  initForm() {
    this.userInfoForm = new FormGroup({
      id: new FormControl(''),      
      userId: new FormControl('', Validators.required),
      userName: new FormControl('', Validators.required),
      userPassword: new FormControl('', Validators.required),
      panNo: new FormControl('', Validators.required),
      adharCardNo: new FormControl('', Validators.required),
      phoneNo: new FormControl('', Validators.required),
      email: new FormControl('', Validators.required),
      address: new FormControl('', Validators.required),
      stateId: new FormControl('', Validators.required),
      nationId: new FormControl('', Validators.required)
    })
  }


  saveUser() {
    if (this.userInfoForm.valid) {
      let payload = {
        "userId": this.userInfoForm.get('userId')?.value ?? 0,
        "userName": this.userInfoForm.get('userName')?.value,
        "userPassword": this.userInfoForm.get('userPassword')?.value,
        "email": this.userInfoForm.get('email')?.value,
        "panNo": this.userInfoForm.get('panNo')?.value,
        "adharCardNo": this.userInfoForm.get('adharCardNo')?.value,
        "phoneNo": this.userInfoForm.get('phoneNo')?.value,
        "address": this.userInfoForm.get('address')?.value,
        "stateId": this.userInfoForm.get('stateId')?.value,
        "nationId": this.userInfoForm.get('nationId')?.value,
        "isActive": true
      }
      this.service.createNewUser(payload).subscribe({
        next: (res: any) => {
          debugger
          if (res) {
            this.commonService.showSnackbar(res.message, 'SUCCESS', 3000)
            this.dialogRef.close(true)
          }
        },
        error: (err: any) => {
          this.commonService.showSnackbar(err.message, 'ERROR', 3000)
        }
      })
    }
    else {
      const message = "Fill the mandatory fields"
      this.commonService.showSnackbar(message, 'ERROR', 3000)
    }
  }

  updateUser() {
    if (this.userInfoForm.valid) {
      let payload = {
        "id": this.userInfoForm.get('id')?.value ?? 0,
        "userId": this.userInfoForm.get('userId')?.value ??  0,
        "userName": this.userInfoForm.get('userName')?.value,
        "userPassword": this.userInfoForm.get('userPassword')?.value,
        "email": this.userInfoForm.get('email')?.value,
        "panNo": this.userInfoForm.get('panNo')?.value,
        "adharCardNo": this.userInfoForm.get('adharCardNo')?.value,
        "phoneNo": this.userInfoForm.get('phoneNo')?.value,
        "address": this.userInfoForm.get('address')?.value,
        "stateId": this.userInfoForm.get('stateId')?.value,
        "nationId": this.userInfoForm.get('nationId')?.value,
        "isActive": true
      }
      this.service.updateUser(this.userInfoForm.get('id')?.value, payload).subscribe({
        next: (res: any) => {
          debugger
          if (res) {
            this.commonService.showSnackbar(res.message, 'SUCCESS', 3000)
            this.dialogRef.close(true)
          }
        },
        error: (err: any) => {
          this.commonService.showSnackbar(err.message, 'ERROR', 3000)
        }
      })
    }
    else {
      const message = "Fill the mandatory fields"
      this.commonService.showSnackbar(message, 'ERROR', 3000),
      this.userInfoForm.markAllAsTouched()
    }
  }
}
