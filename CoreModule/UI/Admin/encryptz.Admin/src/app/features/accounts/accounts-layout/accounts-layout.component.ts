import { CdkDrag } from '@angular/cdk/drag-drop';
import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterOutlet } from '@angular/router';
import { AccountsSideBarComponent } from "./accounts-side-bar/accounts-side-bar.component";
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BusinessContextService, SelectedBusiness } from '../../../core/services/business-context.service';
import { CommonService } from '../../../shared/services/common.service';
import { MatChipsModule } from '@angular/material/chips';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-accounts-layout',
  imports: [
    RouterOutlet,
    CdkDrag,
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatSidenavModule,
    MatFormFieldModule,
    MatSelectModule,
    MatChipsModule,
    MatMenuModule,
    MatTooltipModule,
    AccountsSideBarComponent
],
  templateUrl: './accounts-layout.component.html',
  styleUrl: './accounts-layout.component.scss'
})
export class AccountsLayoutComponent implements OnInit {

  isSmallDevice: boolean = false
  isSidebarExpanded: boolean = true;
  selectedBusiness: SelectedBusiness | null = null;

  constructor(private breakpointObserver: BreakpointObserver
    , private businessContext: BusinessContextService,
    private commonService: CommonService,
    private router: Router,
    private destroyRef: DestroyRef,
    private authService: AuthService
  ) {
    this.breakpointObserver.observe(['(max-width: 767.99px)']).subscribe(result => {
      if (result.matches) {
        this.isSmallDevice = true
      }
      else {
        this.isSmallDevice = false
      }
    })
  }

  ngOnInit(): void {
    if (!this.authService.isProfileComplete()) {
      this.router.navigate(['/profile-setup']);
      return;
    }

    this.businessContext.selectedBusiness$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(business => {
        this.selectedBusiness = business;
        if (!business) {
          this.commonService.showSnackbar('Select a business to continue.', 'INFO', 3000);
          this.router.navigate(['/dashboard']);
        }
      });
  }

  toggleSidebar() {
    this.isSidebarExpanded = !this.isSidebarExpanded;
  }
  checkExpandNav(event: any) {
    this.isSidebarExpanded = event
  }

  navigateBack(): void {
    this.businessContext.clearBusinessContext();
    this.router.navigate(['/dashboard']);
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.commonService.showSnackbar('Logged out successfully', 'SUCCESS', 2000);
        this.router.navigate(['/']);
      },
      error: () => {
        // Even if logout fails, clear session and redirect
        this.router.navigate(['/']);
      }
    });
  }

  get currentUser() {
    return this.authService.getUser();
  }
}
