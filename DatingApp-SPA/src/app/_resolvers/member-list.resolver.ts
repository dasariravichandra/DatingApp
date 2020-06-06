import { Injectable } from '@angular/core';
import {User} from '../_models/user';
import { UserService } from '../_services/user.service';
import { Router, ActivatedRouteSnapshot, Resolve } from '@angular/router';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()

export class MemberlistResolver implements Resolve<User[]>{
    constructor(private userService: UserService, private router: Router, private alertify: AlertifyService){}

        resolve(route: ActivatedRouteSnapshot): Observable<User[]>{
            return this.userService.getUsers().pipe(
                catchError(error => {
                    this.alertify.Error('problem retrieving Data');
                    this.router.navigate(['/home']);
                    return of(null);
                })
            );
        }
}