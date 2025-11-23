import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { finalize } from 'rxjs';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { AdminActivity, AdminDashboardSummary, AdminPlanUsage } from '../../../../core/models/admin.models';
import { CommonService } from '../../../../shared/services/common.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatTableModule,
    MatButtonModule,
    MatChipsModule
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent {
  summary: AdminDashboardSummary | null = null;
  loading = false;

  displayedUserColumns = ['userHandle', 'fullName', 'email', 'created'];
  displayedBusinessColumns = ['businessName', 'location', 'created'];
  displayedActivityColumns = ['table', 'action', 'user', 'date'];
  displayedPlanColumns = ['plan', 'price', 'active', 'status'];

  constructor(
    private adminDataService: AdminDataService,
    private commonService: CommonService
  ) { }

  ngOnInit(): void {
    this.loadSummary();
  }

  loadSummary(): void {
    this.loading = true;
    this.commonService.loaderState(true);

    this.adminDataService.getDashboardSummary()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.commonService.loaderState(false);
        })
      )
      .subscribe({
        next: summary => {
          this.summary = summary;
        },
        error: () => {
          this.summary = null;
        }
      });
  }

  trackActivity(_index: number, activity: AdminActivity): number {
    return activity.auditLogId;
  }

  trackPlan(_index: number, plan: AdminPlanUsage): number {
    return plan.planId;
  }
}
