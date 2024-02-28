/// <reference types="cypress" />



describe('Test cases for Employee table', () => {

    const baseUrl = 'https://localhost:7101/Employee'

    beforeEach(() => {
      cy.visit(baseUrl)
    })

    it('Check URL for valid status code', () => {
        cy.log('Should return a valid 200 status code')

        cy.request(baseUrl).then((response) => {
            expect(response.status).to.eq(200)
        })
    })


    
    
    it('Verify that table exist and is visible', () => {
        cy.log('Verify that table exist and is visible')

        cy.get('h1').should('exist').and('be.visible')
        cy.get('table').should('exist').and('be.visible')
    })

    it('Should display search bar', () => {
        cy.log('Should display search bar')

        cy.get('#searchString').should('exist').and('be.visible')
    })

    context('Search functionality', () => {

        it('Should display valid search results', () => {
            cy.performSearch('Tamoy Smith');
            cy.get('table').should('contain', 'Tamoy Smith');
            cy.get('table.styled-table td').contains('Tamoy Smith').should('be.visible');
            cy.get('table.styled-table td').contains('90,9').should('be.visible');
        });

        it('Should display message for invalid search results', () => {
            cy.performSearch('aaa');
            cy.contains('No data available.').should('be.visible');
        });
    });
 
})


describe('Test case for picture', () => {

     it('Verify that picture exist', () => {
         cy.request('https://localhost:7101/EmployeesGraph')
     })
     
 })

