import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { BusinessContextService, SelectedBusiness } from '../../../../core/services/business-context.service';
import { CommonService } from '../../../../shared/services/common.service';

interface AccountsMenuCard {
  title: string;
  description: string;
  icon: string;
  route?: string;
  comingSoon?: boolean;
}

@Component({
  selector: 'app-accounts-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule
  ],
  templateUrl: './accounts-dashboard.component.html',
  styleUrl: './accounts-dashboard.component.scss'
})
export class AccountsDashboardComponent {
  business$: Observable<SelectedBusiness | null>;

  menuCards: AccountsMenuCard[] = [
    {
      title: 'Chart of Accounts',
      description: 'Manage the complete ledger hierarchy for your business.',
      icon: 'account_tree',
      route: '/accounts/chartofaccounts'
    },
    {
      title: 'Vouchers',
      description: 'Record purchases, sales, receipts, and payments.',
      icon: 'receipt_long',
      comingSoon: true
    },
    {
      title: 'Ledger Reports',
      description: 'Dive into ledgers to trace every transaction.',
      icon: 'library_books',
      comingSoon: true
    },
    {
      title: 'Trial Balance',
      description: 'Validate books before generating statutory reports.',
      icon: 'analytics',
      comingSoon: true
    }
  ];

  constructor(
    private businessContext: BusinessContextService,
    private commonService: CommonService,
    private router: Router
  ) {
    this.business$ = this.businessContext.selectedBusiness$;
  }

  openMenu(card: AccountsMenuCard): void {
    if (card.comingSoon || !card.route) {
      this.commonService.showSnackbar('Module is under development.', 'INFO', 2500);
      return;
    }
    this.router.navigateByUrl(card.route);
  }
}
