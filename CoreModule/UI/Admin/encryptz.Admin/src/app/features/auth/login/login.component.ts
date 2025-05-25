import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { SlickCarouselModule } from 'ngx-slick-carousel';




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
    SlickCarouselModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  email: string = '';
  password: string = '';

  // slides = [
  //   { img: "http://placehold.it/350x150/000000" },
  //   { img: "http://placehold.it/350x150/111111" },
  //   { img: "http://placehold.it/350x150/333333" },
  //   { img: "http://placehold.it/350x150/666666" }
  // ];
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


  constructor(private router: Router) { }

  onLogin() {
    if (this.email === 'admin@example.com' && this.password === 'password') {
      alert('Login Successful');
      this.router.navigate(['/dashboard']);
    } else {
      alert('Invalid Credentials');
    }
  }
}
