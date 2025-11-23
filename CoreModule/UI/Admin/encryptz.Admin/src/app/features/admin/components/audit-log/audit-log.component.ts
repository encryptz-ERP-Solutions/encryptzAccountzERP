import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { AuditLogEntry, AuditLogFilter } from '../../../../core/models/admin.models';
import { CommonService } from '../../../../shared/services/common.service';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    ReactiveFormsModule
  ],
  templateUrl: './audit-log.component.html',
  styleUrl: './audit-log.component.scss'
})
export class AuditLogComponent {
  logs: AuditLogEntry[] = [];
  displayedColumns = ['tableName', 'action', 'user', 'date', 'recordId'];
  filterForm!: FormGroup;

  constructor(
    private adminDataService: AdminDataService,
    private fb: FormBuilder,
    private commonService: CommonService
  ) {
    this.filterForm = this.fb.group({
      tableName: [''],
      action: [''],
      startDate: [''],
      endDate: [''],
      limit: [50]
    });
  }

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    const raw = this.filterForm.getRawValue();
    const filter: AuditLogFilter = {
      tableName: raw.tableName || undefined,
      action: raw.action || undefined,
      limit: raw.limit || 50,
      startDateUtc: raw.startDate ? new Date(raw.startDate).toISOString() : undefined,
      endDateUtc: raw.endDate ? new Date(raw.endDate).toISOString() : undefined
    };

    this.adminDataService.getAuditLogs(filter).subscribe({
      next: logs => this.logs = logs,
      error: err => this.commonService.showSnackbar(err?.message || 'Unable to load audit logs', 'ERROR', 3000)
    });
  }

  resetFilters(): void {
    this.filterForm.reset({
      tableName: '',
      action: '',
      startDate: '',
      endDate: '',
      limit: 50
    });
    this.loadLogs();
  }
}

