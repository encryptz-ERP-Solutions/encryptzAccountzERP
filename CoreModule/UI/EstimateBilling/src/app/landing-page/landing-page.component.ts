import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  styleUrl: './landing-page.component.scss'
})
export class LandingPageComponent implements OnInit{
  showLoading = true;

  ngOnInit() {
    // Simulate loading delay
    setTimeout(() => {
      this.showLoading = false;
    }, 1000); // Adjust the time as needed
  }
}
