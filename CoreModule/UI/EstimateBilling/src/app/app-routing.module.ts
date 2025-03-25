import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LandingPageComponent } from './landing-page/landing-page.component';
import { HeaderComponent } from './header/header.component';
import { LayoutComponent } from './layout/layout.component';

const routes: Routes = [
  { path: '', component: LandingPageComponent }, // Default route
  { path: 'invoice', component: LayoutComponent }, // Route for the About page
  
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
