import { Component } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { SlickCarouselModule } from 'ngx-slick-carousel';
import { CommonService } from '../../../shared/services/common.service';

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
    private common : CommonService
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
}
