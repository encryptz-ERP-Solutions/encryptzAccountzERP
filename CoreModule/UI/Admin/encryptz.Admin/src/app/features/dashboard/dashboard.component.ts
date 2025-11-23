import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { BusinessContextService, SelectedBusiness } from '../../core/services/business-context.service';
import { UserBusinessService, UserBusinessSummary } from '../../core/services/user-business.service';
import { CommonService } from '../../shared/services/common.service';
import { AdminDataService } from '../../core/services/admin-data.service';
import { AddEditBusinessComponent } from '../admin/components/business-management/add-edit-business/add-edit-business.component';

interface ModuleCard {
  key: string;
  title: string;
  description: string;
  icon: string;
  route: string;
  badge?: string;
  comingSoon?: boolean;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    MatTooltipModule,
    MatDialogModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  greetingName = 'there';
  businesses: UserBusinessSummary[] = [];
  selectedBusiness: SelectedBusiness | null = null;
  isLoadingBusinesses = false;
  loadError = '';
  modules: ModuleCard[] = [];
  private readonly baseModules: ModuleCard[] = [
    {
      key: 'accounts',
      title: 'Accounts',
      description: 'Manage chart of accounts, vouchers, ledgers, and statutory reports.',
      icon: 'account_balance_wallet',
      route: '/accounts'
    }
  ];

  constructor(
    private authService: AuthService,
    private userBusinessService: UserBusinessService,
    private businessContextService: BusinessContextService,
    private commonService: CommonService,
    private router: Router,
    private destroyRef: DestroyRef,
    private dialog: MatDialog,
    private adminDataService: AdminDataService
  ) { }

  ngOnInit(): void {
    const user = this.authService.getUser();

    if (!user?.userId) {
      this.commonService.showSnackbar('Session expired. Please login again.', 'ERROR', 3000);
      this.router.navigate(['/']);
      return;
    }

    if (!this.authService.isProfileComplete()) {
      this.router.navigate(['/profile-setup']);
      return;
    }

    this.greetingName = user.userName || user.userHandle || 'there';
    this.selectedBusiness = this.businessContextService.currentBusiness;
    this.modules = [...this.baseModules];

    if (this.authService.isSystemAdmin()) {
      this.modules.push({
        key: 'admin',
        title: 'Admin',
        description: 'Manage users, subscriptions, and core configurations.',
        icon: 'admin_panel_settings',
        route: '/admin'
      });
    }

    this.fetchBusinesses(user.userId);
  }

  selectBusiness(business: UserBusinessSummary): void {
    this.setSelectedBusiness(business, true);
  }

  clearSelection(): void {
    this.businessContextService.clearBusinessContext();
    this.selectedBusiness = null;
  }

  openCreateBusinessDialog(): void {
    const dialogRef = this.dialog.open(AddEditBusinessComponent, {
      width: '600px',
      data: { mode: 'create', useMyBusinessEndpoint: true }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Refresh businesses list after creating a new business
        const user = this.authService.getUser();
        if (user?.userId) {
          this.fetchBusinesses(user.userId);
        }
      }
    });
  }

  openModule(module: ModuleCard): void {
    if (!this.selectedBusiness) {
      this.commonService.showSnackbar('Please select a business to continue.', 'INFO', 3000);
      return;
    }

    if (module.comingSoon) {
      this.commonService.showSnackbar('Module coming soon.', 'INFO', 3000);
      return;
    }

    this.businessContextService.setSelectedModule(module.key);
    this.router.navigate([module.route]);
  }

  private fetchBusinesses(userId: string): void {
    this.isLoadingBusinesses = true;
    this.userBusinessService.getUserBusinesses(userId)
      .pipe(
        finalize(() => this.isLoadingBusinesses = false),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: businesses => {
          this.businesses = businesses;
          if (!businesses.length) {
            this.clearSelection();
            return;
          }

          if (this.selectedBusiness) {
            const stillExists = businesses.find(b => b.userBusinessID === this.selectedBusiness?.userBusinessID);
            if (!stillExists) {
              this.setSelectedBusiness(businesses.find(b => b.isDefault) ?? businesses[0], false);
            }
            return;
          }

          const defaultBusiness = businesses.find(b => b.isDefault) ?? businesses[0];
          this.setSelectedBusiness(defaultBusiness, false);
        },
        error: err => {
          this.loadError = err?.error?.message || 'Unable to load your businesses.';
          this.commonService.showSnackbar(this.loadError, 'ERROR', 4000);
        }
      });
  }

  private setSelectedBusiness(business: UserBusinessSummary, notify: boolean): void {
    if (!business) {
      return;
    }

    const normalized: SelectedBusiness = {
      userBusinessID: business.userBusinessID,
      businessID: business.businessID,
      businessName: business.businessName,
      businessCode: business.businessCode,
      isDefault: business.isDefault
    };

    this.selectedBusiness = normalized;
    this.businessContextService.setSelectedBusiness(normalized);

    if (notify) {
      const title = business.businessName || 'Business';
      this.commonService.showSnackbar(`${title} selected`, 'SUCCESS', 2500);
    }
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
