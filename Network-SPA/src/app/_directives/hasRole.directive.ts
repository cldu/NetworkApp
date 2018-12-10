import { Directive, Input, ViewContainerRef, TemplateRef, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Directive({
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[];
  isVisible = false;

  constructor(private viewContainerRef: ViewContainerRef, private templateRef: TemplateRef<any>, private authService: AuthService) { }

  ngOnInit() {
    const userRoles = this.authService.decodedToken.role;
    // don't show if no roles provided
    if (!userRoles) {
      this.viewContainerRef.clear();
    }

    if (this.authService.roleMatch(this.appHasRole)) {
      if (!this.isVisible) {
        this.isVisible = true;
        this.viewContainerRef.createEmbeddedView(this.templateRef); // templateRef refers to element with structural directive
      } else {
        this.isVisible = false;
        this.viewContainerRef.clear();
      }
    }

    }
  }
