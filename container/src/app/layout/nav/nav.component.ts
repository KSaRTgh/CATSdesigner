import {Component, OnDestroy, OnInit} from "@angular/core";
import {LayoutService} from "../layout.service";
import {AuthenticationService} from "../../core/services/auth.service";
import {first, takeUntil, tap} from "rxjs/operators";
import {CoreService} from "../../core/services/core.service";
import {Subject} from "rxjs";
import { Lecturer, Student, Group } from '../../core/models/searchResults/search-results';
import { SearchService } from '../../core/services/searchResults/search.service';
import { ProfileService } from '../../core/services/searchResults/profile.service';
import { MenuService } from "src/app/core/services/menu.service";
import { MatDialog } from "@angular/material/dialog";
import {AboutSystemPopoverComponent} from '../../about-system/about-popover/about-popover.component';


interface Locale {
  name: string;
  value: string
}


@Component({
  selector: "app-nav",
  templateUrl: "./nav.component.html",
  styleUrls: ["./nav.component.less"]
})
export class NavComponent implements OnInit, OnDestroy {
  public isLector: boolean;
  public isAdmin: boolean;
  public unconfirmedStudents: number = 0;
  public locales: Locale[] = [{name: "Ru", value: "ru"}, {name: "En", value: "en"}];
  public locale: Locale;
  private unsubscribeStream$: Subject<void> = new Subject<void>();
  public profileIcon = "/assets/images/account.png";;

  public currentUserId!: number;
  valueForSearch!: string;
  
  searchResults !: string[];

  lecturerSearchResults!: Lecturer[];
  studentSearchResults!: Student[];
  groupSearchResults!: Group[];


  constructor(private layoutService: LayoutService,
              private coreService: CoreService,
              private autService: AuthenticationService,
              private searchService: SearchService,
              private profileService: ProfileService,
              private menuService: MenuService,
              public dialog: MatDialog
              ) {
  }

  get logoWidth(): string {
    const width = this.menuService.getSideNavWidth();
    return width ? `${width - 16}px` : 'auto';
  }

  public ngOnInit(): void {
    this.isLector = this.autService.currentUserValue.role == "lector";
    this.isAdmin = this.autService.currentUserValue.role == "admin";
    this.getAvatar();
    const local: string = localStorage.getItem("locale");
    this.locale = local ? this.locales.find((locale: Locale) => locale.value === local) : this.locales[0];


    this.currentUserId = this.autService.currentUserValue.id;

    this.coreService.getGroups()
      .pipe(
        tap((groups: any) => {
          if (groups && groups.Groups) {
            groups.Groups.forEach((group: any) => {
              this.unconfirmedStudents += group.CountUnconfirmedStudents;
            });
          }
        }),
        takeUntil(this.unsubscribeStream$)
      ).subscribe();
  }

  public sidenavAction(): void {
    this.layoutService.toggle();
  }

  public logOut(): void {
    this.autService.logout().pipe(first()).subscribe(
      () => location.reload());
  }

  public onValueChange(value: any): void {
    localStorage.setItem("locale", value.value.value);
    window.location.reload()
  }

  public ngOnDestroy(): void {
    this.unsubscribeStream$.next(null);
    this.unsubscribeStream$.complete();
  }



  getAvatar() {
    this.profileService.getAvatar().subscribe(res => {
      this.profileIcon = res;
    });
  }

  viewSearchResults() {
    this.valueForSearch = this.valueForSearch.trim();
    if (this.valueForSearch.length >= 3) {
      this.viewGroupSearchResults();
      this.viewLecturerSearchResults();
      this.viewStudentSearchResults();
    }
    else {
      this.cleanSearchResults();
    }
  }

  cleanSearchResults() {
    this.lecturerSearchResults = null;
    this.studentSearchResults = null;
    this.groupSearchResults = null;
  }

  viewLecturerSearchResults() {
    this.searchService.getLecturerSearchResults(this.valueForSearch).subscribe(res => {
       this.lecturerSearchResults = res;
    });
  }

  viewStudentSearchResults() {
    this.searchService.getStudentSearchResults(this.valueForSearch).subscribe(res => {
      this.studentSearchResults = res;
    });
  }

  viewGroupSearchResults() {
    this.searchService.getGroupSearchResults(this.valueForSearch).subscribe(res => {
      this.groupSearchResults = res;
    });
  }

  public routeToAboutPopover() {

    const dialogRef = this.dialog.open(AboutSystemPopoverComponent, {
      width: '600px',
      height: '60%', 
      position: {top: '128px'}
    }); 

    dialogRef.afterClosed().subscribe(result => {
      if (result != null) {
      }
})

    
}

}
