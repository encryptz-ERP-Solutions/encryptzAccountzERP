import { Component, ElementRef, QueryList, ViewChildren } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { SlickCarouselModule } from 'ngx-slick-carousel';
import { CommonService } from '../../../shared/services/common.service';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    FormsModule,
    MatInputModule,
    MatFormFieldModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    SlickCarouselModule,
    ReactiveFormsModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  email = new FormControl('', Validators.required)
  password = new FormControl('', Validators.required)
  emailOTP = new FormControl('', Validators.required)
  fullNameOTP = new FormControl('')
  panOTP = new FormControl('')

  enableEmailLogin: boolean = false
  isOTPValidation: boolean = false
  isVerifyOTP: boolean = false
  otpFields = ['otpFirst', 'otpSecond', 'otpThird', 'otpFourth', 'otpFifth', 'otpSixth'];
  @ViewChildren('otpInput') otpInputs!: QueryList<ElementRef>;
  otpLoginForm !: FormGroup

  slideConfig = {
    slidesToShow: 1,
    slidesToScroll: 1,
    autoplay: true,
    autoplaySpeed: 3000,
    arrows: false,
    dots: true,
    infinite: true,
    pauseOnHover: false,
    pauseOnFocus: false,
    pauseOnDotsHover: false
  };


  constructor(
    private router: Router,
    private common: CommonService,
    private fb: FormBuilder,
    private authService: AuthService
  ) { }

  onLogin() {
    if (this.email.valid && this.password.valid) {
      const message = 'Log In Successfully'
      this.common.showSnackbar(message, 'SUCCESS', 3000)
      this.router.navigate(['/dashboard']);
    } else {
      const message = 'Please enter valid user name and password'
      this.common.showSnackbar(message, 'ERROR', 3000)
    }
  }

  getOTP() {
    if (this.emailOTP.valid) {
      let payload = {
        "loginType": "Email",
        "loginId": this.emailOTP.value,
        "fullName": this.fullNameOTP.value,
        "panNo": this.panOTP.value
      }

      this.authService.sendOTP(payload).subscribe({
        next: (res: any) => {
          if (res.status) {
            this.isOTPValidation = true
            this.generateOTPLoginForm()
            setTimeout(() => {
              this.otpInputs.first.nativeElement.focus();
            }, 100);
          }
        },
        error: (err: any) => {

        }
      })
    }
    else {
      this.emailOTP.markAsTouched()
    }
  }

  verifyOTP() {
    if (this.otpLoginForm.valid) {
      const otp = Object.values(this.otpLoginForm.value).join('');
      let payload = {
        "loginType": "Email",
        "loginId": this.emailOTP.value,
        "fullName": this.fullNameOTP.value,
        "panNo": this.panOTP.value,
        "otp": otp
      }

      this.authService.verifyOTP(payload).subscribe({
        next: (res: any) => {
          if (res.status) {
            debugger
          }
        },
        error: (err: any) => {

        }
      })
    }
    else {
      return;
    }

  }

  generateOTPLoginForm() {
    this.otpLoginForm = this.fb.group({
      otpFirst: ['', Validators.required],
      otpSecond: ['', Validators.required],
      otpThird: ['', Validators.required],
      otpFourth: ['', Validators.required],
      otpFifth: ['', Validators.required],
      otpSixth: ['', Validators.required],
    })
  }


  onOtpInput(event: any, index: number) {
    const input = event.target
    const value = input.value
    if (value.length > 1) {
      input.value = value[0];
    }
    if (value) {
      const inputArray = this.otpInputs.toArray();
      if (index < inputArray.length - 1) {
        const nextInput = inputArray[index + 1];
        nextInput?.nativeElement.focus();
      }
    }

    if (this.otpLoginForm.valid) this.isVerifyOTP = true
    else this.isVerifyOTP = false
  }

  onOtpKeyDown(event: KeyboardEvent, index: number) {
    const input = (event.target as HTMLInputElement)
    if (event.key === 'Backspace') {
      if (!input.value && index > 0) {
        const previousInput = this.otpInputs.get(index - 1)
        previousInput?.nativeElement.focus()
      }
    }
  }

  onOtpPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const pasteData = event.clipboardData?.getData('text') ?? '';
    const digits = pasteData.replace(/\D/g, '').slice(0, 6).split('');
    digits.forEach((digit, i) => {
      if (i < this.otpFields.length) {
        this.otpLoginForm.get(this.otpFields[i])?.setValue(digit);
      }
    });
    const nextEmptyIndex = digits.length < 6 ? digits.length : 5;
    this.otpInputs.get(nextEmptyIndex)?.nativeElement.focus();
    if (this.otpLoginForm.valid) this.isVerifyOTP = true
    else this.isVerifyOTP = false
  }
}
