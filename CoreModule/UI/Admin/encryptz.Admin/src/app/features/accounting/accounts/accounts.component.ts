import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CdkDrag, CdkDragEnd } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';


@Component({
  selector: 'app-accounts',
  imports: [
    RouterOutlet,
    CdkDrag,
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule
  ],
  templateUrl: './accounts.component.html',
  styleUrl: './accounts.component.scss',
  standalone: true
})
export class AccountsComponent {

  top = 0;
  left = 0;
  isDropped = false;

  onDragEnd(event: CdkDragEnd): void {
    const rect = (event.source.getRootElement() as HTMLElement).getBoundingClientRect();

    this.top = rect.top;
    this.left = rect.left;
    this.isDropped = true;

    // Optionally disable future drags
    // event.source.disabled = true;
  }

  //   onDragEnd(event: CdkDragEnd) {
  //   const element = event.source.element.nativeElement as HTMLElement;

  //   // Get current transform
  //   const transform = event.source.getFreeDragPosition();

  //   // Clear the transform style set by cdkDrag
  //   element.style.transform = 'none';

  //   // Apply the drag position as margin (or padding, or any other way to shift layout)
  //   element.style.marginLeft = `${transform.x}px`;
  //   element.style.marginTop = `${transform.y}px`;

  //   // Disable further dragging if needed:
  //   // event.source.disabled = true;
  // }
}
