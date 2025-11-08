import { ChangeDetectorRef, Component, ElementRef, QueryList, ViewChildren } from '@angular/core';
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
  forgotPasswordMail = new FormControl('', Validators.required)
  newPassword = new FormControl('', Validators.required)
  fullNameOTP = new FormControl('')
  panOTP = new FormControl('')

  isVisible: boolean = false
  newPasswordVisible: boolean = false
  enableEmailLogin: boolean = false
  isOTPValidation: boolean = false
  isVerifyOTP: boolean = false
  isForgotPassword: boolean = false
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
    private authService: AuthService,
    private cd: ChangeDetectorRef
  ) { }

  onLogin() {
    if (this.email.valid && this.password.valid) {
      let payload = {
        "loginIdentifier": this.email.value,
        "password": this.password.value,
      }

      this.authService.login(payload).subscribe({
        next: (res: any) => {
          if (res.isSuccess) {
            this.router.navigate(['/dashboard'])
            this.common.showSnackbar('Logged In Successfully', 'SUCCESS', 3000);
          } else {
            const errorMessage = res?.message || 'Login failed. Please try again.';
            this.common.showSnackbar(errorMessage, 'ERROR', 3000);
          }
        },
        error: (err: any) => {
          const errorMessage = err?.error?.message || err?.message || 'Login failed. Please check your credentials.';
          this.common.showSnackbar(errorMessage, 'ERROR', 3000)
        }
      })
    } else {
      const message = 'Please enter user name and password'
      this.common.showSnackbar(message, 'ERROR', 3000)
    }
  }

  requestOTP() {
    if (this.emailOTP.valid) {
      let payload = {
        "loginIdentifier": this.emailOTP.value,
        "otpMethod": 'email'
      }

      this.authService.requestOTP(payload).subscribe({
        next: (res: any) => {
          if (res.status) {
            this.isOTPValidation = true
            this.generateOTPLoginForm()
            this.cd.detectChanges();
            this.otpInputs.first?.nativeElement.focus();
          } else {
            const errorMessage = res?.message || 'Failed to send OTP. Please try again.';
            this.common.showSnackbar(errorMessage, 'ERROR', 3000);
          }
        },
        error: (err: any) => {
          const errorMessage = err?.error?.message || err?.message || 'Failed to send OTP. Please try again.';
          this.common.showSnackbar(errorMessage, 'ERROR', 3000);
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
        "loginIdentifier": this.emailOTP.value,
        "otp": otp
      }

      this.authService.verifyOTP(payload).subscribe({
        next: (res: any) => {
          if (res.isSuccess) {
            this.router.navigate(['/dashboard'])
            this.common.showSnackbar('Logged In Successfully', 'SUCCESS', 3000);
          } else {
            const errorMessage = res?.message || 'OTP verification failed. Please try again.';
            this.common.showSnackbar(errorMessage, 'ERROR', 3000);
          }
        },
        error: (err: any) => {
          const errorMessage = err?.error?.message || err?.message || 'OTP verification failed. Please try again.';
          this.common.showSnackbar(errorMessage, 'ERROR', 3000);
        }
      })
    }
    else {
      this.otpLoginForm.markAllAsTouched()
      return;
    }
  }

  requestForgotOTP() {
    if (this.forgotPasswordMail.valid) {
      let payload = {
        "loginIdentifier": this.forgotPasswordMail.value
      }

      this.authService.requestForgotOTP(payload).subscribe({
        next: (res: any) => {
          if (res.status) {
            this.isOTPValidation = true
            this.generateOTPLoginForm()
            this.cd.detectChanges();
            this.otpInputs.first?.nativeElement.focus();
          } else {
            const errorMessage = res?.message || 'Failed to send OTP. Please try again.';
            this.common.showSnackbar(errorMessage, 'ERROR', 3000);
          }
        },
        error: (err: any) => {
          const errorMessage = err?.error?.message || err?.message || 'Failed to send OTP. Please try again.';
          this.common.showSnackbar(errorMessage, 'ERROR', 3000);
        }
      })
    }
    else {
      this.forgotPasswordMail.markAsTouched()
    }
  }

  resetPassword() {
    if (this.otpLoginForm.valid && this.newPassword.valid) {
      const otp = Object.values(this.otpLoginForm.value).join('');
      let payload = {
        "loginIdentifier": this.forgotPasswordMail.value,
        "otp": otp,
        "newPassword": this.newPassword.value
      }

      this.authService.resetPassword(payload).subscribe({
        next: (res: any) => {
          if (res.status) {
            this.router.navigate(['/dashboard'])
            this.common.showSnackbar('Logged In Successfully', 'SUCCESS', 3000);
          } else {
            const errorMessage = res?.message || 'Password reset failed. Please try again.';
            this.common.showSnackbar(errorMessage, 'ERROR', 3000);
          }
        },
        error: (err: any) => {
          const errorMessage = err?.error?.message || err?.message || 'Password reset failed. Please try again.';
          this.common.showSnackbar(errorMessage, 'ERROR', 3000);
        }
      })
    }
    else {
      this.newPassword.markAsTouched()
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
    this.isVerifyOTP = false;
  }


  onOtpInput(event: any, index: number) {
    const input = event.target as HTMLInputElement;
    const raw = input.value || '';
    const char = raw.replace(/\s/g, '').charAt(0) || '';

    // update the native input to single char
    input.value = char;

    // update the form control explicitly
    const controlName = this.otpFields[index];
    this.otpLoginForm.get(controlName)?.setValue(char, { emitEvent: false });

    // focus next
    const inputs = this.otpInputs.toArray();
    if (char && index < inputs.length - 1) {
      (inputs[index + 1].nativeElement as HTMLInputElement).focus();
    }
    this.updateVerifyOTPState();
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
    this.updateVerifyOTPState();
  }

  private updateVerifyOTPState(): void {
    if (this.otpLoginForm) {
      this.isVerifyOTP = this.otpLoginForm.valid;
    } else {
      this.isVerifyOTP = false;
    }
  }

  resetOtpState() {
    this.isOTPValidation = false;
    this.isVerifyOTP = false;
    this.isForgotPassword = false
    if (this.otpLoginForm) {
      this.otpLoginForm.reset();
    }
  }
}
